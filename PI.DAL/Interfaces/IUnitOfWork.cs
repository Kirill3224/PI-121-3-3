namespace PI.DAL.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    Task CompleteAsync(CancellationToken cancellationToken = default);
}