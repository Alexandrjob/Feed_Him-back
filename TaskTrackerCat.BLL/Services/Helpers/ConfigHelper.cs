using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.BLL.Services.Helpers;

public class ConfigHelper
{
    public DateTime GetStartTimeFeeding(DateTime estimatedDateFeeding, ConfigDto newConfig)
    {
        //Устанавливаем значения часа и минут в начальные значения.
        return new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            newConfig.StartFeeding.Hours,
            newConfig.StartFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    public DateTime GetEndTimeFeeding(DateTime estimatedDateFeeding, ConfigDto newConfig)
    {
        return new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            newConfig.EndFeeding.Hours,
            newConfig.EndFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    public TimeSpan GetIntervalFeeding(ConfigDto config)
    {
        var numberMealsPerDay = config.NumberMealsPerDay;
        var timeFeeding = config.EndFeeding - config.StartFeeding;
        //Вычитание происходит из-за того, что последний прием записывается от конфига.
        var notRoundedInterval = timeFeeding / (numberMealsPerDay - 1);

        //Код взят с сайта. https://kkblog.ru/rounding-datetime-datestamp/
        //Округляем в меньшую сторону.
        var sec = notRoundedInterval.TotalSeconds;
        var divider = 5 * 60;
        //выравниваем секунды по началу интервала.
        var newSec = Math.Floor(sec / divider) * divider;
        //переводим секунды в такты.
        var newTicks = (long) newSec * 10000000;

        return new TimeSpan(newTicks);
    }
}