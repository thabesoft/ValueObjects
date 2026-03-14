using System.Diagnostics.CodeAnalysis;

namespace ThabeSoft.ValueObjects.Cn;

/// <summary>
/// 中国行政区划代码 (6位数字) 的内部解析模型。
/// 遵循 GB/T 2260 中华人民共和国行政区划代码标准。
/// </summary>
public record CnAddressCode : IParsable<CnAddressCode>, ISpanParsable<CnAddressCode>
{
    /// <summary>
    /// 完整的 6 位行政区划代码。
    /// 由省、市、县三级代码按顺序拼接而成。
    /// </summary>
    /// <example>410328</example>
    public required string FullCode { get; init; }

    /// <summary>
    /// 省级行政区代码（前 2 位）。
    /// 代表省、自治区、直辖市、特别行政区。
    /// 例如：41 代表河南省。
    /// </summary>
    public required string ProvinceCode { get; init; }

    /// <summary>
    /// 地级行政区代码（中间 2 位）。
    /// 代表地级市、地区、自治州、盟。
    /// 例如：03 代表洛阳市。
    /// </summary>
    public required string CityCode { get; init; }

    /// <summary>
    /// 县级行政区代码（最后 2 位）。
    /// 代表市辖区、县级市、县、旗。
    /// 例如：28 代表伊川县。
    /// </summary>
    public required string DistrictCode { get; init; }



    /// <summary>
    /// 表示一个空的行政区划代码。
    /// </summary>
    public static readonly CnAddressCode Empty = new()
    {
        FullCode = "000000",
        ProvinceCode = "00",
        CityCode = "00",
        DistrictCode = "00"
    };

    private CnAddressCode() { }

    /// <summary>
    /// 从指定的 6 位行政区划代码创建实例。
    /// </summary>
    /// <param name="code">6 位数字代码。</param>
    /// <returns>行政区划代码对象。</returns>
    /// <exception cref="ArgumentException">代码格式不正确时抛出。</exception>
    public static CnAddressCode FromCode(string code)
    {
        return Parse(code, null);
    }
    /// <summary>
    /// 支持从 CnAddressCode 隐式转换为 string
    /// </summary>
    public static implicit operator string(CnAddressCode? code)
    {
        return code?.FullCode ?? string.Empty;
    }
    /// <summary>
    /// 支持从 string 隐式转换为 CnAddressCode
    /// </summary>
    public static implicit operator CnAddressCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return Empty;
        return Parse(code, null);
    }


    /// <summary>
    /// 尝试从字符跨度（Span）解析行政区划代码。
    /// 这是解析的核心逻辑，通过零分配（Zero-allocation）方式提高性能。
    /// </summary>
    /// <param name="s">包含 6 位数字代码的只读字符序列。</param>
    /// <param name="provider">一个提供特定格式信息的对象（在此实现中通常忽略）。</param>
    /// <param name="result">如果解析成功，则包含解析后的 <see cref="CnAddressCode"/> 实例；否则为 null。</param>
    /// <returns>如果解析成功则为 true；否则为 false。</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [NotNullWhen(true)] out CnAddressCode? result)
    {
        result = null;
        var input = s.Trim(); // 移除首尾空白字符

        // 校验：必须精准为 6 位数字
        if (input.Length != 6) return false;

        foreach (char c in input)
        {
            if (!char.IsDigit(c)) return false;
        }

        // 构造模型，将 Span 转换为最终存储的字符串
        result = new CnAddressCode
        {
            FullCode = input.ToString(),
            ProvinceCode = input[..2].ToString(),  // 提取前2位：省级
            CityCode = input[2..4].ToString(),     // 提取中2位：地级
            DistrictCode = input[4..].ToString()   // 提取后2位：县级
        };

        return true;
    }
    /// <summary>
    /// 解析字符跨度为行政区划代码，如果格式错误则抛出异常。
    /// </summary>
    /// <exception cref="FormatException">当输入格式不符合 6 位数字要求时抛出。</exception>
    public static CnAddressCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result : throw new FormatException("无效的中国行政区划代码格式。");
    }

    /// <summary>
    /// 尝试从字符串解析行政区划代码。
    /// 内部将字符串转换为 Span 以复用高性能解析逻辑。
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out CnAddressCode? result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }
    /// <summary>
    /// 解析字符串为行政区划代码，如果格式错误则抛出异常。
    /// </summary>
    public static CnAddressCode Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }

    /// <summary>
    /// 返回6位地址码
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return FullCode;
    }
}