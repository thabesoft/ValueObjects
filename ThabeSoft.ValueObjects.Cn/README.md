# ThabeSoft.ValueObjects.Cn

> 提供了一些中国特色值对象

## 身份证

支持 18 位校验及 15 位自动升位

```C#
// 从字符串初始化
CnIdCard idCard = "18或15位身份证";
// 从字符串转换
CnIdCard idCard = CnIdCard.Parse("");
```

## 地址码

```C#
// 从字符串初始化
CnAddressCode code = "410328";

// 访问拆解后的代码
var p = code.ProvinceCode; // 41
```

## 性别

```C#
CnGender gender = CnGender.Male;
CnGender gender = CnGender.Female;
```