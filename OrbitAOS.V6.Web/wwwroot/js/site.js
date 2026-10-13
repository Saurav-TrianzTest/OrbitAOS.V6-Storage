// OrbitAOS Book Management - Site JavaScript
// Migrated from legacy ASP.NET MVC to ASP.NET Core MVC .NET 8
// Static files now served from wwwroot (replaces legacy /Scripts folder)

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});
