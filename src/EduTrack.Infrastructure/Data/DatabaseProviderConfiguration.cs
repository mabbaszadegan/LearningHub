using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Infrastructure.Data;

public interface IDatabaseProviderConfiguration
{
    void ConfigureLongText<T>(PropertyBuilder<string?> propertyBuilder) where T : class;
    void ConfigureMediumText<T>(PropertyBuilder<string?> propertyBuilder) where T : class;
    void ConfigureIdentityColumn<T>(PropertyBuilder<int> propertyBuilder) where T : class;
    void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset> propertyBuilder) where T : class;
    void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset?> propertyBuilder) where T : class;
}

public class SqlServerConfiguration : IDatabaseProviderConfiguration
{
    public void ConfigureLongText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("nvarchar(max)");
    }

    public void ConfigureMediumText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("nvarchar(4000)");
    }

    public void ConfigureIdentityColumn<T>(PropertyBuilder<int> propertyBuilder) where T : class
    {
        propertyBuilder.UseIdentityColumn();
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("datetimeoffset");
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("datetimeoffset");
    }
}

public class SqliteConfiguration : IDatabaseProviderConfiguration
{
    public void ConfigureLongText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("TEXT");
    }

    public void ConfigureMediumText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("TEXT");
    }

    public void ConfigureIdentityColumn<T>(PropertyBuilder<int> propertyBuilder) where T : class
    {
        propertyBuilder.ValueGeneratedOnAdd();
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("TEXT");
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("TEXT");
    }
}

public class PostgreSqlConfiguration : IDatabaseProviderConfiguration
{
    public void ConfigureLongText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("text");
    }

    public void ConfigureMediumText<T>(PropertyBuilder<string?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("varchar(4000)");
    }

    public void ConfigureIdentityColumn<T>(PropertyBuilder<int> propertyBuilder) where T : class
    {
        propertyBuilder.UseIdentityByDefaultColumn();
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("timestamptz");
    }

    public void ConfigureDateTimeOffset<T>(PropertyBuilder<DateTimeOffset?> propertyBuilder) where T : class
    {
        propertyBuilder.HasColumnType("timestamptz");
    }
}

public static class DatabaseProviderFactory
{
    public static IDatabaseProviderConfiguration GetConfiguration(string providerName)
    {
        return providerName.ToLower() switch
        {
            "microsoft.entityframeworkcore.sqlserver" => new SqlServerConfiguration(),
            "microsoft.entityframeworkcore.sqlite" => new SqliteConfiguration(),
            "npgsql.entityframeworkcore.postgresql" => new PostgreSqlConfiguration(),
            _ => new SqlServerConfiguration() // Default fallback
        };
    }
}
