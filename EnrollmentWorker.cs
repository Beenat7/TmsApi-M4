using Microsoft.Extensions.DependencyInjection;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();

        var enrollmentService =
            scope.ServiceProvider
                .GetRequiredService<IEnrollmentService>();

        Console.WriteLine("Processing enrollments...");
    }
}







// public class EnrollmentWorker
// {
//     private readonly IEnrollmentService _enrollmentService;//This is intentionally wrong, The lab wants us to see ASP.NET catch it

//     public EnrollmentWorker(IEnrollmentService enrollmentService)
//     {
//         _enrollmentService = enrollmentService;
//     }

//     public void ProcessBatch()
//     {
//         Console.WriteLine("Processing enrollments...");
//     }
// }