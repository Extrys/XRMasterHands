using System;

public interface IStatePerformer
{
	event Action<bool> PerformStateChanged;
}