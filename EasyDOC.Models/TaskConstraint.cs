namespace EasyDOC.Model
{
    public enum TaskConstraint
    {
        AsSoonAsPossible,
        AsLateAsPossible,
        FinishNoEarlierThan,
        FinishNoLaterThan,
        MustStartOn,
        MustFinishOn,
        StartNoEarlierThan,
        StartNoLaterThan
    }
}