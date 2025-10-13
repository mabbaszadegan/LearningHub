using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Extensions;

public static class ScheduleItemTypeExtensions
{
    public static string GetDisplayName(this ScheduleItemType type)
    {
        return type switch
        {
            ScheduleItemType.Reminder => "یادآوری",
            ScheduleItemType.Writing => "نوشتاری",
            ScheduleItemType.Audio => "صوتی",
            ScheduleItemType.GapFill => "پر کردن جای خالی",
            ScheduleItemType.MultipleChoice => "چند گزینه‌ای",
            ScheduleItemType.Match => "تطبیق",
            ScheduleItemType.ErrorFinding => "پیدا کردن خطا",
            ScheduleItemType.CodeExercise => "تمرین کد",
            ScheduleItemType.Quiz => "کویز",
            _ => "نامشخص"
        };
    }

    public static string GetDescription(this ScheduleItemType type)
    {
        return type switch
        {
            ScheduleItemType.Reminder => "ایجاد محتوای یادآوری با قابلیت افزودن متن، تصویر، ویدیو و صوت",
            ScheduleItemType.Writing => "تمرین نوشتاری برای دانش‌آموزان",
            ScheduleItemType.Audio => "تمرینات صوتی و شنیداری",
            ScheduleItemType.GapFill => "تمرین پر کردن جای خالی در متن",
            ScheduleItemType.MultipleChoice => "سوالات چند گزینه‌ای",
            ScheduleItemType.Match => "تمرین تطبیق و ارتباط",
            ScheduleItemType.ErrorFinding => "تمرین پیدا کردن خطا در متن",
            ScheduleItemType.CodeExercise => "تمرینات برنامه‌نویسی و کدنویسی",
            ScheduleItemType.Quiz => "کویز و آزمون کوتاه",
            _ => "نوع آیتم نامشخص"
        };
    }

    public static bool HasContentBuilder(this ScheduleItemType type)
    {
        return type == ScheduleItemType.Reminder;
    }
}
