using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Services.Validators;

/// <summary>
/// Factory for getting the appropriate validator based on schedule item type
/// </summary>
public class BlockAnswerValidatorFactory
{
    private readonly IEnumerable<IBlockAnswerValidator> _validators;

    public BlockAnswerValidatorFactory(IEnumerable<IBlockAnswerValidator> validators)
    {
        _validators = validators;
    }

    public IBlockAnswerValidator GetValidator(ScheduleItemType type)
    {
        var validator = _validators.FirstOrDefault(v => v.SupportedType == type);
        if (validator == null)
        {
            throw new NotSupportedException($"No validator found for schedule item type: {type}");
        }

        return validator;
    }
}

