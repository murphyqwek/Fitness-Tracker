namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchDTO(int Id, string Name, string Description, IList<ExerciseMuscleDTO> Muscles)
    {
    }

    public record ExerciseSearchReducedDTO(int Id, string Name, IList<ExerciseMuscleDTO> Muscles)
    {
    }

    public record ExerciseMuscleDTO(int Id, string Name, decimal PercentageOfUsage) 
    {
    }
}
