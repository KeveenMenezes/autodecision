
namespace AutodecisionCore.Contracts.Enums;

public enum FlagResultEnum
{
    InProcessing = 0,        // é o status de criaçaõ.. 
    Processed = 1,      // processou e tudo ok
    Ignored = 2,        // há uma regra para ignorar
    PendingApproval = 3, // "flag levantada"
    AutoDeny = 4,        // Automaticamente declinar
    Approved = 5,        // Aprovado.
    Error = 6,
    Warning = 7
}
