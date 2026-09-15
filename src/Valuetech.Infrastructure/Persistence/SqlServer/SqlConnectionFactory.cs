using Microsoft.Data.SqlClient;

namespace Valuetech.Infrastructure.Persistence.SqlServer;

internal sealed class SqlConnectionFactory(string connectionString)
{
    internal const string ConnectionStringName = "SqlServer";

    public SqlConnection CreateConnection() => new(connectionString);
}
