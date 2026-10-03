using System.Buffers.Binary;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Application.Abstractions.Orders;

namespace Sweet_Silvia_Infrastructure.Abstractions.Orders;

internal sealed class PostgresUserCheckoutLock(ApplicationDbContext context) : IUserCheckoutLock
{
    public async Task<IAsyncDisposable> AcquireAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = context.Database.GetDbConnection();
        var closeOnDispose = connection.State == ConnectionState.Closed;
        if (closeOnDispose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var key = BinaryPrimitives.ReadInt64BigEndian(SHA256.HashData(userId.ToByteArray()));
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT pg_advisory_lock(@key)";
            AddKeyParameter(command, key);
            await command.ExecuteScalarAsync(cancellationToken);
            return new CheckoutLockHandle(connection, key, closeOnDispose);
        }
        catch
        {
            if (closeOnDispose)
            {
                await connection.CloseAsync();
            }

            throw;
        }
    }

    private static void AddKeyParameter(DbCommand command, long key)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = "key";
        parameter.Value = key;
        command.Parameters.Add(parameter);
    }

    private sealed class CheckoutLockHandle(DbConnection connection, long key, bool closeOnDispose) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT pg_advisory_unlock(@key)";
                AddKeyParameter(command, key);
                await command.ExecuteScalarAsync(CancellationToken.None);
            }
            finally
            {
                if (closeOnDispose)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
