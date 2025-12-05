namespace OffroadCamping.Appointments.SharedKernel.BusinessRulesEngine;

public class BusinessRuleValidationException : InvalidOperationException
{
    public BusinessRuleValidationException(string message) : base(message)
    {
    }
}