using Microsoft.Maui.Storage;

namespace CropTrackApp.Services;

public static class MeasurementService
{
    private const decimal HectaresPerAcre = 0.40468564224m;
    private const decimal KilogramsPerTon = 1000m;
    private const decimal PoundsPerTon = 2204.62262185m;
    private const decimal MillimetersPerInch = 25.4m;

    public static int TemperatureUnitIndex => Preferences.Get("temperature_unit_index", 0);
    public static int AreaUnitIndex => Preferences.Get("area_unit_index", 0);
    public static int WeightUnitIndex => Preferences.Get("weight_unit_index", 0);
    public static int RainfallUnitIndex => Preferences.Get("rainfall_unit_index", 0);

    public static string TemperatureUnit => TemperatureUnitIndex == 1 ? "F" : "C";
    public static string AreaUnit => AreaUnitIndex == 1 ? "ha" : "acres";
    public static string WeightUnit => WeightUnitIndex switch
    {
        1 => "kg",
        2 => "lb",
        _ => "tons"
    };
    public static string RainfallUnit => RainfallUnitIndex == 1 ? "in" : "mm";

    public static decimal FromCelsius(decimal celsius)
    {
        return TemperatureUnitIndex == 1
            ? (celsius * 9m / 5m) + 32m
            : celsius;
    }

    public static decimal ToCelsius(decimal value)
    {
        return TemperatureUnitIndex == 1
            ? (value - 32m) * 5m / 9m
            : value;
    }

    public static decimal FromAcres(decimal acres)
    {
        return AreaUnitIndex == 1 ? acres * HectaresPerAcre : acres;
    }

    public static decimal ToAcres(decimal value)
    {
        return AreaUnitIndex == 1 ? value / HectaresPerAcre : value;
    }

    public static decimal FromTons(decimal tons)
    {
        return WeightUnitIndex switch
        {
            1 => tons * KilogramsPerTon,
            2 => tons * PoundsPerTon,
            _ => tons
        };
    }

    public static decimal ToTons(decimal value)
    {
        return WeightUnitIndex switch
        {
            1 => value / KilogramsPerTon,
            2 => value / PoundsPerTon,
            _ => value
        };
    }

    public static decimal PricePerPreferredWeight(decimal pricePerTon)
    {
        return WeightUnitIndex switch
        {
            1 => pricePerTon / KilogramsPerTon,
            2 => pricePerTon / PoundsPerTon,
            _ => pricePerTon
        };
    }

    public static decimal PriceToPerTon(decimal price)
    {
        return WeightUnitIndex switch
        {
            1 => price * KilogramsPerTon,
            2 => price * PoundsPerTon,
            _ => price
        };
    }

    public static decimal FromMillimeters(decimal millimeters)
    {
        return RainfallUnitIndex == 1 ? millimeters / MillimetersPerInch : millimeters;
    }

    public static decimal ToMillimeters(decimal value)
    {
        return RainfallUnitIndex == 1 ? value * MillimetersPerInch : value;
    }

    public static string FormatTemperature(decimal celsius)
    {
        return $"{FromCelsius(celsius):0.#}{TemperatureUnit}";
    }

    public static string FormatArea(decimal acres)
    {
        return $"{FromAcres(acres):0.##} {AreaUnit}";
    }

    public static string FormatWeight(decimal tons)
    {
        return $"{FromTons(tons):0.##} {WeightUnit}";
    }

    public static string FormatRainfall(decimal millimeters)
    {
        return $"{FromMillimeters(millimeters):0.#} {RainfallUnit}";
    }
}
