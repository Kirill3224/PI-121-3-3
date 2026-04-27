namespace PI.DAL.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    Task CompleteAsync(CancellationToken cancellationToken = default);
}