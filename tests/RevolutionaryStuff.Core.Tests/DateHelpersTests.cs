using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class DateHelpersTests
{
    #region Existing Tests

    [TestMethod]
    public void TestAgeOffsetToday()
    {
        var today = DateTime.Today;
        const int expected = 45;

        var dt45 = today.AddYears(-expected);
        Assert.AreEqual(today.Year - expected, dt45.Year);
        Assert.AreEqual(today.Month, dt45.Month);
        Assert.AreEqual(today.Day, dt45.Day);
        Assert.AreEqual(expected, dt45.Age());
        Assert.AreEqual(expected, dt45.AddDays(-1).Age());
        Assert.AreEqual(expected, dt45.AddMonths(-1).Age());
        Assert.AreEqual(expected - 1, dt45.AddDays(1).Age());
        Assert.AreEqual(expected - 1, dt45.AddMonths(1).Age());
        Assert.AreEqual(expected - 1, dt45.AddMonths(1).AddDays(1 + today.Day).Age());
    }

    [TestMethod]
    public void DateTimeToIsoStringTests()
    {
        var dt = DateTime.Now;
        var s = dt.ToIsoString();
        var dtViaRsllcParse = Parse.ParseNullableDateTime(s);
        Assert.IsNotNull(dtViaRsllcParse);
        Assert.AreEqual(dt, dtViaRsllcParse);
        var dtViaDotNetParse = DateTime.Parse(s);
        Assert.AreEqual(dt, dtViaDotNetParse);
    }

    [TestMethod]
    public void DateTimeOffsetToIsoStringTests()
    {
        var dto = DateTimeOffset.Now;
        var s = dto.ToIsoString();
        var dtoViaRsllcParse = Parse.ParseNullableDateTimeOffset(s);
        Assert.IsNotNull(dtoViaRsllcParse);
        Assert.AreEqual(dto, dtoViaRsllcParse);
        var dtoViaDotNetParse = DateTimeOffset.Parse(s);
        Assert.AreEqual(dto, dtoViaDotNetParse);
    }

    [TestMethod]
    public void DateTimeOffsetToIsoStringWithLegacyZTests()
    {
        var dto = DateTimeOffset.Now;
        var s = dto.ToIsoString();
        if (!s.EndsWith("Z"))
        {
            s += "Z";
        }
        var dtoViaRsllcParse = Parse.ParseNullableDateTimeOffset(s);
        Assert.IsNotNull(dtoViaRsllcParse);
        Assert.AreEqual(dto, dtoViaRsllcParse);
        try
        {
            var dtoViaDotNetParse = DateTimeOffset.Parse(s);
            Assert.AreEqual(dto, dtoViaDotNetParse);
        }
        catch (System.FormatException fex)
        {
            //because this was BUSTED to add in the "Z"
        }
    }

    #endregion

    #region Unix Epoch Tests

    [TestMethod]
    public void DateTimeFromUnixEpoch_Zero_ReturnsEpoch()
    {
        var result = DateHelpers.DateTimeFromUnixEpoch(0);
        Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), result);
        Assert.AreEqual(DateTimeKind.Utc, result.Kind);
    }

    [TestMethod]
    public void DateTimeFromUnixEpoch_PositiveValue()
    {
        var result = DateHelpers.DateTimeFromUnixEpoch(1609459200); // 2021-01-01 00:00:00 UTC
        Assert.AreEqual(2021, result.Year);
        Assert.AreEqual(1, result.Month);
        Assert.AreEqual(1, result.Day);
    }

    [TestMethod]
    public void DateTimeOffsetFromUnixEpoch_Zero_ReturnsEpoch()
    {
        var result = DateHelpers.DateTimeOffsetFromUnixEpoch(0);
        Assert.AreEqual(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero), result);
    }

    [TestMethod]
    public void DateTimeOffsetFromUnixEpoch_PositiveValue()
    {
        var result = DateHelpers.DateTimeOffsetFromUnixEpoch(1609459200);
        Assert.AreEqual(2021, result.Year);
        Assert.AreEqual(1, result.Month);
        Assert.AreEqual(1, result.Day);
    }

    #endregion

    #region GetFirstSpecifiedFileDate Tests

    [TestMethod]
    public void GetFirstSpecifiedFileDate_AllNull_ReturnsEarliestFileDate()
    {
        var result = DateHelpers.GetFirstSpecifiedFileDate(null, null, null);
        Assert.AreEqual(DateHelpers.UnixEarliestFileDate, result);
    }

    [TestMethod]
    public void GetFirstSpecifiedFileDate_FirstNonNull_ReturnsFirst()
    {
        var date1 = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var date2 = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var result = DateHelpers.GetFirstSpecifiedFileDate(null, date1, date2);
        Assert.AreEqual(date1, result);
    }

    #endregion

    #region IsWeekday/IsWeekend Tests - DateTime

    [TestMethod]
    public void IsWeekday_DateTime_Monday_ReturnsTrue()
    {
        var monday = new DateTime(2024, 1, 1); // Monday
        Assert.IsTrue(monday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekday_DateTime_Saturday_ReturnsFalse()
    {
        var saturday = new DateTime(2024, 1, 6); // Saturday
        Assert.IsFalse(saturday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekday_DateTime_Sunday_ReturnsFalse()
    {
        var sunday = new DateTime(2024, 1, 7); // Sunday
        Assert.IsFalse(sunday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekend_DateTime_Saturday_ReturnsTrue()
    {
        var saturday = new DateTime(2024, 1, 6);
        Assert.IsTrue(saturday.IsWeekend());
    }

    [TestMethod]
    public void IsWeekend_DateTime_Monday_ReturnsFalse()
    {
        var monday = new DateTime(2024, 1, 1);
        Assert.IsFalse(monday.IsWeekend());
    }

    #endregion

    #region IsWeekday/IsWeekend Tests - DateOnly

    [TestMethod]
    public void IsWeekday_DateOnly_Friday_ReturnsTrue()
    {
        var friday = new DateOnly(2024, 1, 5);
        Assert.IsTrue(friday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekday_DateOnly_Sunday_ReturnsFalse()
    {
        var sunday = new DateOnly(2024, 1, 7);
        Assert.IsFalse(sunday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekend_DateOnly_Saturday_ReturnsTrue()
    {
        var saturday = new DateOnly(2024, 1, 6);
        Assert.IsTrue(saturday.IsWeekend());
    }

    #endregion

    #region IsWeekday/IsWeekend Tests - DateTimeOffset

    [TestMethod]
    public void IsWeekday_DateTimeOffset_Tuesday_ReturnsTrue()
    {
        var tuesday = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        Assert.IsTrue(tuesday.IsWeekday());
    }

    [TestMethod]
    public void IsWeekend_DateTimeOffset_Sunday_ReturnsTrue()
    {
        var sunday = new DateTimeOffset(2024, 1, 7, 0, 0, 0, TimeSpan.Zero);
        Assert.IsTrue(sunday.IsWeekend());
    }

    #endregion

    #region ToMilitaryTime Tests

    [TestMethod]
    public void ToMilitaryTime_TimeOnly_WithSeconds()
    {
        var time = new TimeOnly(14, 30, 45);
        Assert.AreEqual("14:30:45", time.ToMilitaryTime());
    }

    [TestMethod]
    public void ToMilitaryTime_TimeOnly_WithoutSeconds()
    {
        var time = new TimeOnly(14, 30, 45);
        Assert.AreEqual("14:30", time.ToMilitaryTime(includeSeconds: false));
    }

    [TestMethod]
    public void ToMilitaryTime_DateTime_WithSeconds()
    {
        var dt = new DateTime(2024, 1, 1, 14, 30, 45);
        Assert.AreEqual("14:30:45", dt.ToMilitaryTime());
    }

    [TestMethod]
    public void ToMilitaryTime_DateTime_Midnight()
    {
        var dt = new DateTime(2024, 1, 1, 0, 0, 0);
        Assert.AreEqual("00:00:00", dt.ToMilitaryTime());
    }

    [TestMethod]
    public void ToMilitaryTime_DateTimeOffset_WithSeconds()
    {
        var dt = new DateTimeOffset(2024, 1, 1, 23, 59, 59, TimeSpan.Zero);
        Assert.AreEqual("23:59:59", dt.ToMilitaryTime());
    }

    #endregion

    #region ToYYYY_MM_DD Tests

    [TestMethod]
    public void ToYYYY_MM_DD_DateTime()
    {
        var dt = new DateTime(2024, 1, 15);
        Assert.AreEqual("2024-01-15", dt.ToYYYY_MM_DD());
    }

    [TestMethod]
    public void ToYYYY_MM_DD_DateOnly()
    {
        var dt = new DateOnly(2024, 12, 31);
        Assert.AreEqual("2024-12-31", dt.ToYYYY_MM_DD());
    }

    [TestMethod]
    public void ToYYYY_MM_DD_DateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        Assert.AreEqual("2024-06-15", dt.ToYYYY_MM_DD());
    }

    #endregion

    #region ToYYYYMMDD Tests

    [TestMethod]
    public void ToYYYYMMDD_DateTime()
    {
        var dt = new DateTime(2024, 1, 5);
        Assert.AreEqual("20240105", dt.ToYYYYMMDD());
    }

    [TestMethod]
    public void ToYYYYMMDD_DateOnly()
    {
        var dt = new DateOnly(2024, 12, 25);
        Assert.AreEqual("20241225", dt.ToYYYYMMDD());
    }

    [TestMethod]
    public void ToYYYYMMDD_DateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.Zero);
        Assert.AreEqual("20240310", dt.ToYYYYMMDD());
    }

    #endregion

    #region ToHHMMSS Tests

    [TestMethod]
    public void ToHHMMSS_DateTime()
    {
        var dt = new DateTime(2024, 1, 1, 14, 30, 45);
        Assert.AreEqual("143045", dt.ToHHMMSS());
    }

    [TestMethod]
    public void ToHHMMSS_TimeOnly()
    {
        var time = new TimeOnly(9, 5, 3);
        Assert.AreEqual("090503", time.ToHHMMSS());
    }

    [TestMethod]
    public void ToHHMMSS_DateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 1, 1, 23, 59, 59, TimeSpan.Zero);
        Assert.AreEqual("235959", dt.ToHHMMSS());
    }

    #endregion

    #region ToRfc7231 Tests

    [TestMethod]
    public void ToRfc7231_DateTime()
    {
        var dt = new DateTime(2008, 10, 1, 17, 4, 32, DateTimeKind.Utc);
        var result = dt.ToRfc7231();
        Assert.IsTrue(result.Contains("01 Oct 2008"));
        Assert.IsTrue(result.Contains("17:04:32"));
        Assert.IsTrue(result.Contains("GMT"));
    }

    [TestMethod]
    public void ToRfc7231_DateTimeOffset()
    {
        var dt = new DateTimeOffset(2008, 10, 1, 17, 4, 32, TimeSpan.Zero);
        var result = dt.ToRfc7231();
        Assert.IsTrue(result.Contains("01 Oct 2008"));
        Assert.IsTrue(result.Contains("GMT"));
    }

    #endregion

    #region ToIsoString Additional Tests

    [TestMethod]
    public void ToIsoString_DateOnly()
    {
        var dt = new DateOnly(2008, 10, 1);
        var result = dt.ToIsoString();
        Assert.AreEqual("2008-10-01", result);
    }

    [TestMethod]
    public void ToIsoString_TimeOnly()
    {
        var time = new TimeOnly(17, 4, 32);
        var result = time.ToIsoString();
        Assert.IsTrue(result.StartsWith("17:04:32"));
    }

    #endregion

    #region Age Tests - DateOnly

    [TestMethod]
    public void Age_DateOnly_ExactYears()
    {
        var birthDate = new DateOnly(2000, 1, 1);
        var asOf = new DateOnly(2024, 1, 1);
        Assert.AreEqual(24, birthDate.Age(asOf));
    }

    [TestMethod]
    public void Age_DateOnly_BeforeBirthday()
    {
        var birthDate = new DateOnly(2000, 6, 15);
        var asOf = new DateOnly(2024, 3, 1);
        Assert.AreEqual(23, birthDate.Age(asOf));
    }

    [TestMethod]
    public void Age_DateOnly_AfterBirthday()
    {
        var birthDate = new DateOnly(2000, 3, 15);
        var asOf = new DateOnly(2024, 6, 1);
        Assert.AreEqual(24, birthDate.Age(asOf));
    }

    #endregion

    #region Age Tests - DateTimeOffset

    [TestMethod]
    public void Age_DateTimeOffset_ExactYears()
    {
        var birthDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var asOf = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.AreEqual(24, birthDate.Age(asOf));
    }

    #endregion

    #region TotalMillisecondsToInt Tests

    [TestMethod]
    public void TotalMillisecondsToInt_WholeNumber()
    {
        var ts = TimeSpan.FromMilliseconds(1000);
        Assert.AreEqual(1000, ts.TotalMillisecondsToInt());
    }

    [TestMethod]
    public void TotalMillisecondsToInt_RoundsUp()
    {
        var ts = TimeSpan.FromMilliseconds(1000.6);
        Assert.AreEqual(1001, ts.TotalMillisecondsToInt());
    }

    [TestMethod]
    public void TotalMillisecondsToInt_RoundsDown()
    {
        var ts = TimeSpan.FromMilliseconds(1000.4);
        Assert.AreEqual(1000, ts.TotalMillisecondsToInt());
    }

    #endregion

    #region Conversion Tests - ToDateOnly

    [TestMethod]
    public void ToDateOnly_FromDateTime()
    {
        var dt = new DateTime(2024, 1, 15, 14, 30, 45);
        var dateOnly = dt.ToDateOnly();
        Assert.AreEqual(2024, dateOnly.Year);
        Assert.AreEqual(1, dateOnly.Month);
        Assert.AreEqual(15, dateOnly.Day);
    }

    [TestMethod]
    public void ToDateOnly_FromDateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        var dateOnly = dt.ToDateOnly();
        Assert.AreEqual(2024, dateOnly.Year);
        Assert.AreEqual(6, dateOnly.Month);
        Assert.AreEqual(15, dateOnly.Day);
    }

    #endregion

    #region Conversion Tests - ToTimeOnly

    [TestMethod]
    public void ToTimeOnly_FromDateTime()
    {
        var dt = new DateTime(2024, 1, 15, 14, 30, 45);
        var timeOnly = dt.ToTimeOnly();
        Assert.AreEqual(14, timeOnly.Hour);
        Assert.AreEqual(30, timeOnly.Minute);
        Assert.AreEqual(45, timeOnly.Second);
    }

    [TestMethod]
    public void ToTimeOnly_FromDateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 1, 15, 23, 59, 59, TimeSpan.Zero);
        var timeOnly = dt.ToTimeOnly();
        Assert.AreEqual(23, timeOnly.Hour);
        Assert.AreEqual(59, timeOnly.Minute);
        Assert.AreEqual(59, timeOnly.Second);
    }

    #endregion

    #region Conversion Tests - ToDateTime

    [TestMethod]
    public void ToDateTime_FromDateOnlyAndTimeOnly()
    {
        var date = new DateOnly(2024, 1, 15);
        var time = new TimeOnly(14, 30, 45);
        var dt = date.ToDateTime(time);

        Assert.AreEqual(2024, dt.Year);
        Assert.AreEqual(1, dt.Month);
        Assert.AreEqual(15, dt.Day);
        Assert.AreEqual(14, dt.Hour);
        Assert.AreEqual(30, dt.Minute);
        Assert.AreEqual(45, dt.Second);
    }

    [TestMethod]
    public void ToDateTime_WithKind_Utc()
    {
        var date = new DateOnly(2024, 1, 15);
        var time = new TimeOnly(14, 30, 0);
        var dt = date.ToDateTime(time, DateTimeKind.Utc);

        Assert.AreEqual(DateTimeKind.Utc, dt.Kind);
    }

    #endregion

    #region Conversion Tests - ToDateTimeOffset

    [TestMethod]
    public void ToDateTimeOffset_FromDateOnlyAndTimeOnly()
    {
        var date = new DateOnly(2024, 1, 15);
        var time = new TimeOnly(14, 30, 45);
        var dto = date.ToDateTimeOffset(time);

        Assert.AreEqual(2024, dto.Year);
        Assert.AreEqual(1, dto.Month);
        Assert.AreEqual(15, dto.Day);
        Assert.AreEqual(14, dto.Hour);
        Assert.AreEqual(30, dto.Minute);
        Assert.AreEqual(45, dto.Second);
        Assert.AreEqual(TimeSpan.Zero, dto.Offset);
    }

    [TestMethod]
    public void ToDateTimeOffset_WithCustomOffset()
    {
        var date = new DateOnly(2024, 1, 15);
        var time = new TimeOnly(14, 30, 0);
        var offset = TimeSpan.FromHours(-5);
        var dto = date.ToDateTimeOffset(time, offset);

        Assert.AreEqual(offset, dto.Offset);
    }

    #endregion

    #region Round-Trip Conversion Tests

    [TestMethod]
    public void RoundTrip_DateTime_ToDateOnly_ToDateTime()
    {
        var original = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc);
        var dateOnly = original.ToDateOnly();
        var timeOnly = original.ToTimeOnly();
        var roundTrip = dateOnly.ToDateTime(timeOnly, original.Kind);

        Assert.AreEqual(original.Year, roundTrip.Year);
        Assert.AreEqual(original.Month, roundTrip.Month);
        Assert.AreEqual(original.Day, roundTrip.Day);
        Assert.AreEqual(original.Hour, roundTrip.Hour);
        Assert.AreEqual(original.Minute, roundTrip.Minute);
        Assert.AreEqual(original.Second, roundTrip.Second);
        Assert.AreEqual(original.Kind, roundTrip.Kind);
    }

    [TestMethod]
    public void RoundTrip_DateTimeOffset_ToDateOnly_ToDateTimeOffset()
    {
        var offset = TimeSpan.FromHours(-5);
        var original = new DateTimeOffset(2024, 6, 15, 10, 30, 45, offset);
        var dateOnly = original.ToDateOnly();
        var timeOnly = original.ToTimeOnly();
        var roundTrip = dateOnly.ToDateTimeOffset(timeOnly, offset);

        Assert.AreEqual(original.Year, roundTrip.Year);
        Assert.AreEqual(original.Month, roundTrip.Month);
        Assert.AreEqual(original.Day, roundTrip.Day);
        Assert.AreEqual(original.Hour, roundTrip.Hour);
        Assert.AreEqual(original.Minute, roundTrip.Minute);
        Assert.AreEqual(original.Second, roundTrip.Second);
        Assert.AreEqual(original.Offset, roundTrip.Offset);
    }

    #endregion
}
