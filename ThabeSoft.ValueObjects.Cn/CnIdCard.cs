using System.Diagnostics.CodeAnalysis;

namespace ThabeSoft.ValueObjects.Cn;


/// <summary>
/// 中国居民身份证值对象 (GB11643-1999)
/// </summary>
public record CnIdCard : ISpanParsable<CnIdCard>
{
    /// <summary>
    /// 完整的 18 位身份证号码字符串。
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// 行政区划代码（前 6 位）。
    /// 包含省、市、县（区）三级层级信息的解析模型。
    /// </summary>
    public required CnAddressCode AddressCode { get; init; }

    /// <summary>
    /// 出生日期（第 7 到 14 位）。
    /// 使用 <see cref="DateOnly"/> 类型确保仅包含日期信息，不含时间及温差干扰。
    /// </summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// 性别信息（第 17 位）。
    /// 根据 ISO/IEC 5218 及国标标准，通过倒数第二位数字的奇偶性判定。
    /// </summary>
    public required CnGender Gender { get; init; }

    /// <summary>
    /// 校验码（第 18 位）。
    /// 采用 ISO 7064:1983.MOD 11-2 校验算法计算得出。
    /// 取值范围为字符 '0'-'9' 或 'X'（代表数字 10）。
    /// </summary>
    public required char Checksum { get; init; }


    public static CnIdCard Empty { get; } = new()
    {
        Number = "000000000000000000",
        AddressCode = "000000",
        BirthDate = new DateOnly(0, 0, 0),
        Gender = CnGender.Unknown,
        Checksum = '0'
    };

    private CnIdCard() { }

    /// <summary>
    /// 支持从 CnIdCard 隐式转换为 string
    /// </summary>
    public static implicit operator string(CnIdCard? idCard)
    {
        return idCard?.Number ?? string.Empty;
    }
    /// <summary>
    /// 支持从 string 隐式转换为 CnIdCard
    /// </summary>
    public static implicit operator CnIdCard(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return Empty;
        return Parse(number, null);
    }



    /// <summary>
    /// 尝试从 15 位或 18 位身份证号码字符序列初始化 <see cref="CnIdCard"/> 实例。
    /// </summary>
    /// <param name="s">
    /// 包含身份证号码的字符序列。支持以下格式：
    /// <list type="bullet">
    /// <item><description>15 位一代身份证（自动根据 GB/T 11643 升级为 18 位，补全 19 世纪码并计算校验码）。</description></item>
    /// <item><description>18 位二代身份证（包含 ISO 7064:1983.MOD 11-2 校验码）。</description></item>
    /// </list>
    /// </param>
    /// <param name="provider">一个提供特定格式信息的对象（在当前实现中主要用于日期解析，可为 null）。</param>
    /// <param name="result">
    /// 当方法返回 <see langword="true"/> 时，包含解析成功的 <see cref="CnIdCard"/> 实例；
    /// 当方法返回 <see langword="false"/> 时，此参数为 <see langword="null"/>。
    /// </param>
    /// <returns>如果解析成功且通过合法性校验（包括长度、数字格式、日期有效性及校验码）则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    /// <remarks>
    /// 此方法会自动处理输入字符串的 <c>Trim</c> 操作，并对日期进行逻辑校验（如排除 2 月 30 日或未来日期）。
    /// </remarks>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out CnIdCard result)
    {
        result = null;
        var input = s.Trim();

        // 1. 位数识别与标准化（统一转为 18 位字符串）
        string fullNumber;
        if (input.Length == 18)
        {
            fullNumber = input.ToString();
        }
        else if (input.Length == 15)
        {
            if (!TryUpgrade15To18(input, out fullNumber)) return false;
        }
        else
        {
            return false;
        }

        // 2. 此时 fullNumber 必然是 18 位，进入内部统一解析
        return TryParse18Internal(fullNumber.AsSpan(), provider, out result);
    }
    private static bool TryParse18Internal(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out CnIdCard result)
    {
        result = null;

        // 校验基本格式（前 17 位数字，末位数字或 X）
        for (int i = 0; i < 17; i++)
        {
            if (!char.IsDigit(s[i])) return false;
        }

        // 校验 Checksum (MOD 11-2)
        if (!ValidateCheckDigit(s)) return false;

        // 解析地址码
        if (!CnAddressCode.TryParse(s[..6], provider, out var addressCode)) return false;

        // 解析生日
        if (!DateOnly.TryParseExact(s[6..14], "yyyyMMdd", provider, System.Globalization.DateTimeStyles.None, out var birthDate)) return false;
        if (birthDate > DateOnly.FromDateTime(DateTime.Today)) return false;

        // 解析性别（第 17 位，奇男偶女）
        int genderNum = s[16] - '0';
        var gender = (genderNum % 2 != 0) ? CnGender.Male : CnGender.Female;

        // 初始化实例
        result = new CnIdCard
        {
            Number = s.ToString(),
            AddressCode = addressCode,
            BirthDate = birthDate,
            Gender = gender,
            Checksum = char.ToUpper(s[17])
        };

        return true;
    }

    /// <summary>
    /// 解析身份证号码字符序列并返回 <see cref="CnIdCard"/> 实例。
    /// 如果号码无效或校验失败，将抛出异常。
    /// </summary>
    /// <param name="s">
    /// 包含 15 位（一代证）或 18 位（二代证）身份证号码的字符序列。
    /// </param>
    /// <param name="provider">一个提供特定格式信息的对象（可为 null）。</param>
    /// <returns>解析成功后返回初始化完成的 <see cref="CnIdCard"/> 对象。</returns>
    /// <exception cref="ArgumentException">当输入为空或长度不符合 15/18 位要求时抛出。</exception>
    /// <exception cref="FormatException">
    /// 当号码包含非法字符、出生日期逻辑错误（如 2 月 30 日）或 ISO 7064 校验码不匹配时抛出。
    /// </exception>
    /// <remarks>
    /// 此方法是 <see cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out CnIdCard?)"/> 的强制版本。
    /// 建议在确定号码来源可靠时使用，否则建议使用 TryParse 以获得更好的性能表现。
    /// </remarks>
    public static CnIdCard Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result : throw new FormatException("无效的身份证号码格式或校验未通过。");
    }

    /// <summary>
    /// 尝试从 15 位或 18 位身份证号码字符序列初始化 <see cref="CnIdCard"/> 实例。
    /// </summary>
    /// <param name="s">
    /// 包含身份证号码的字符序列。支持以下格式：
    /// <list type="bullet">
    /// <item><description>15 位一代身份证（自动根据 GB/T 11643 升级为 18 位，补全 19 世纪码并计算校验码）。</description></item>
    /// <item><description>18 位二代身份证（包含 ISO 7064:1983.MOD 11-2 校验码）。</description></item>
    /// </list>
    /// </param>
    /// <param name="provider">一个提供特定格式信息的对象（在当前实现中主要用于日期解析，可为 null）。</param>
    /// <param name="result">
    /// 当方法返回 <see langword="true"/> 时，包含解析成功的 <see cref="CnIdCard"/> 实例；
    /// 当方法返回 <see langword="false"/> 时，此参数为 <see langword="null"/>。
    /// </param>
    /// <returns>如果解析成功且通过合法性校验（包括长度、数字格式、日期有效性及校验码）则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    /// <remarks>
    /// 此方法会自动处理输入字符串的 <c>Trim</c> 操作，并对日期进行逻辑校验（如排除 2 月 30 日或未来日期）。
    /// </remarks>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CnIdCard result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }
    /// <summary>
    /// 解析身份证号码字符序列并返回 <see cref="CnIdCard"/> 实例。
    /// 如果号码无效或校验失败，将抛出异常。
    /// </summary>
    /// <param name="s">
    /// 包含 15 位（一代证）或 18 位（二代证）身份证号码的字符序列。
    /// </param>
    /// <param name="provider">一个提供特定格式信息的对象（可为 null）。</param>
    /// <returns>解析成功后返回初始化完成的 <see cref="CnIdCard"/> 对象。</returns>
    /// <exception cref="ArgumentException">当输入为空或长度不符合 15/18 位要求时抛出。</exception>
    /// <exception cref="FormatException">
    /// 当号码包含非法字符、出生日期逻辑错误（如 2 月 30 日）或 ISO 7064 校验码不匹配时抛出。
    /// </exception>
    /// <remarks>
    /// 此方法是 <see cref="TryParse(ReadOnlySpan{char}, IFormatProvider?, out CnIdCard?)"/> 的强制版本。
    /// 建议在确定号码来源可靠时使用，否则建议使用 TryParse 以获得更好的性能表现。
    /// </remarks>
    public static CnIdCard Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }


    
    /// <summary>
    /// 将15位转为18位
    /// </summary>
    /// <param name="s15"></param>
    /// <param name="s18"></param>
    /// <returns></returns>
    private static bool TryUpgrade15To18(ReadOnlySpan<char> s15, out string s18)
    {
        s18 = string.Empty;
        if (s15.Length != 15) return false;

        // 1. 构造 17 位本体码：6位地址 + "19" + 9位后续
        // 使用 ValueStringBuilder 或 Span 拼接以优化性能
        Span<char> temp17 = stackalloc char[17];
        s15[..6].CopyTo(temp17);
        temp17[6] = '1';
        temp17[7] = '9';
        s15[6..].CopyTo(temp17[8..]);

        // 2. 计算校验码
        char checkDigit = CalculateCheckDigit(temp17);

        // 3. 合成 18 位
        s18 = $"{temp17.ToString()}{checkDigit}";
        return true;
    }
    /// <summary>
    /// 校验码计算逻辑
    /// </summary>
    private static char CalculateCheckDigit(ReadOnlySpan<char> id17)
    {
        ReadOnlySpan<int> factors = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
        ReadOnlySpan<char> checkDigits = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (id17[i] - '0') * factors[i];
        }
        return checkDigits[sum % 11];
    }
    /// <summary>
    /// ISO 7064:1983.MOD 11-2 校验码算法
    /// </summary>
    private static bool ValidateCheckDigit(ReadOnlySpan<char> id)
    {
        // 加权因子表
        ReadOnlySpan<int> factors = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
        // 校验码对应表
        ReadOnlySpan<char> checkDigits = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (id[i] - '0') * factors[i];
        }

        int index = sum % 11;
        char expected = checkDigits[index];
        char actual = char.ToUpper(id[17]);

        return actual == expected;
    }
}
