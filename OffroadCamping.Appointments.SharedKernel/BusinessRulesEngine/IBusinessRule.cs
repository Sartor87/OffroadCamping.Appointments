namespace OffroadCamping.Appointments.SharedKernel.BusinessRulesEngine;

public interface IBusinessRule
{
    bool IsMet();
    string Error { get; }
}