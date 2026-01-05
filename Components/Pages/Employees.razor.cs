using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using accapp001.Data;
using Employee = accapp001.Properties.Models.Employee;

namespace accapp001.Components.Pages
{
    public partial class Employees : ComponentBase
    {
        [Inject] private ApplicationDbContext DbContext { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private List<Employee> employees = new();
        private Employee newEmployee = new();
        private bool showAddForm = false;
        private bool isLoading = true;
        private bool isSubmitting = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadEmployeesAsync();
        }

        /// <summary>
        /// Loads all employees from Azure SQL Database using async EF Core methods
        /// </summary>
        private async Task LoadEmployeesAsync()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Load employees ordered by most recent first
                employees = await DbContext.Employees
                    .OrderByDescending(e => e.CreatedDate)
                    .AsNoTracking() // Better performance for read-only operations
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error loading employees: {ex.Message}");
                employees = new List<Employee>(); // Ensure UI doesn't break
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Refreshes the employee list from database
        /// </summary>
        public async Task RefreshEmployeesAsync()
        {
            await LoadEmployeesAsync();
        }

        /// <summary>
        /// Shows the add employee form
        /// </summary>
        private void ShowAddForm()
        {
            showAddForm = true;
            newEmployee = new Employee();
            StateHasChanged();
        }

        /// <summary>
        /// Hides the add employee form and resets the form data
        /// </summary>
        private void HideAddForm()
        {
            showAddForm = false;
            newEmployee = new Employee();
            StateHasChanged();
        }

        /// <summary>
        /// Resets the form data without hiding the form
        /// </summary>
        private void ResetForm()
        {
            newEmployee = new Employee();
            StateHasChanged();
        }

        /// <summary>
        /// Inserts a new employee record into Azure SQL Database
        /// </summary>
        private async Task AddEmployeeAsync()
        {
            try
            {
                isSubmitting = true;
                StateHasChanged();

                // Validate email uniqueness
                var emailExists = await DbContext.Employees
                    .AnyAsync(e => e.Email.ToLower() == newEmployee.Email.ToLower());

                if (emailExists)
                {
                    await ShowAlertAsync("An employee with this email already exists.");
                    return;
                }

                // Set audit fields
                newEmployee.CreatedDate = DateTime.UtcNow;
                newEmployee.LastModified = null; // New record doesn't need LastModified

                // Insert into database
                DbContext.Employees.Add(newEmployee);
                await DbContext.SaveChangesAsync();

                await ShowAlertAsync("Employee added successfully!");

                // Reset form and refresh list
                HideAddForm();
                await LoadEmployeesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                await HandleErrorAsync($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error adding employee: {ex.Message}");
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Toggles the active status of an employee
        /// </summary>
        private async Task ToggleEmployeeStatusAsync(Employee employee)
        {
            try
            {
                // Find the employee in the database to ensure we have the latest version
                var dbEmployee = await DbContext.Employees.FindAsync(employee.Id);
                if (dbEmployee == null)
                {
                    await ShowAlertAsync("Employee not found.");
                    await LoadEmployeesAsync(); // Refresh to remove stale data
                    return;
                }

                // Toggle status and update timestamp
                dbEmployee.IsActive = !dbEmployee.IsActive;
                dbEmployee.UpdateLastModified();

                await DbContext.SaveChangesAsync();

                var status = dbEmployee.IsActive ? "activated" : "deactivated";
                await ShowAlertAsync($"Employee {status} successfully!");

                // Update local list without full reload for better UX
                var localEmployee = employees.FirstOrDefault(e => e.Id == employee.Id);
                if (localEmployee != null)
                {
                    localEmployee.IsActive = dbEmployee.IsActive;
                    localEmployee.LastModified = dbEmployee.LastModified;
                    StateHasChanged();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                await HandleErrorAsync("Employee was modified by another user. Please refresh and try again.");
                await LoadEmployeesAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error updating employee: {ex.Message}");
                await LoadEmployeesAsync(); // Reload to revert any local changes
            }
        }

        /// <summary>
        /// Deletes an employee from the database after confirmation
        /// </summary>
        private async Task DeleteEmployeeAsync(Employee employee)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to delete {employee.Name}? This action cannot be undone.");

                if (!confirmed) return;

                // Find and remove the employee
                var dbEmployee = await DbContext.Employees.FindAsync(employee.Id);
                if (dbEmployee == null)
                {
                    await ShowAlertAsync("Employee not found.");
                    await LoadEmployeesAsync();
                    return;
                }

                DbContext.Employees.Remove(dbEmployee);
                await DbContext.SaveChangesAsync();

                await ShowAlertAsync("Employee deleted successfully!");
                await LoadEmployeesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                await HandleErrorAsync($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync($"Error deleting employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles errors gracefully by logging and showing user feedback
        /// </summary>
        private async Task HandleErrorAsync(string errorMessage)
        {
            // Log error (in production, use proper logging like ILogger)
            Console.WriteLine($"Employee Management Error: {errorMessage}");
            
            // Show user-friendly message
            await ShowAlertAsync(errorMessage);
        }

        /// <summary>
        /// Shows an alert message to the user
        /// </summary>
        private async Task ShowAlertAsync(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("alert", message);
            }
            catch (Exception ex)
            {
                // Fallback if JavaScript is not available
                Console.WriteLine($"Alert: {message} | JS Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes of the DbContext when component is destroyed
        /// </summary>
        public void Dispose()
        {
            // DbContext is injected and managed by DI container, no manual disposal needed
        }
    }
}
