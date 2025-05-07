namespace Core.Interfaces;

public interface IDoorController
{
    Task<bool> UnlockAsync(int doorId);
    Task<bool> LockAsync(int doorId);
}