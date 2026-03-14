namespace ThabeSoft.ValueObjects.Cn;


/// <summary>
/// 中国标准性别值对象 (符合 GB/T 2261.1)
/// </summary>
public sealed record CnGender : IEquatable<CnGender>
{
    /// <summary>
    /// 内部唯一标识：采用标准代码 (1-男, 2-女, 0-未知, 9-未说明)
    /// </summary>
    public int Code { get; init; }
    /// <summary>
    /// 性别名称
    /// </summary>
    public required string Name { get; init; }

    
    
    /// <summary>
    /// 未知
    /// </summary>
    public static readonly CnGender Unknown = new() { Code = 0, Name = "未知" };
    /// <summary>
    /// 男性
    /// </summary>
    public static readonly CnGender Male = new() { Code = 1, Name = "男" };
    /// <summary>
    /// 女性
    /// </summary>
    public static readonly CnGender Female = new() { Code = 2, Name = "女" };
    /// <summary>
    /// 未说明
    /// </summary>
    public static readonly CnGender NotStated = new() { Code = 9, Name = "未说明" };

    private CnGender() { }

    /// <summary>
    /// 从 GB/T 2261.1 码中初始化
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static CnGender FromCode(int code) => code switch
    {
        1 => Male,
        2 => Female,
        9 => NotStated,
        _ => Unknown
    };
    /// <summary>
    /// 支持从 CnGender 隐式转换为 int32
    /// </summary>
    public static implicit operator int(CnGender? code)
    {
        return code?.Code ?? 0;
    }
    /// <summary>
    /// 支持从 int32 隐式转换为 CnGender
    /// </summary>
    public static implicit operator CnGender(int code)
    {
        return FromCode(code);
    }



    public bool Equals(CnGender? other)
    {
        if (other is null) return false;
        return Code == other.Code;
    }
    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }
    public override string ToString()
    {
        return Name;
    }
}