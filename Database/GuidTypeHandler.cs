using System.Data;
using Dapper;

namespace storage.Database;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString().ToLower();
    }

    public override Guid Parse(object value)
    {
        return new Guid((string)value);
    }
}