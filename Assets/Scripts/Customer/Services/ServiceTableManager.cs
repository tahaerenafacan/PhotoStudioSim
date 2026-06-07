using UnityEngine;

public class ServiceTableManager : MonoBehaviour
{
    [SerializeField] private ServiceTable serviceTable;
    
    public ServiceTable TryReserveTable(CustomerController customer)
    {
        if (serviceTable == null) return null;

        return serviceTable.TryReserve(customer) ? serviceTable : null;
    }

    public void ReleaseTable(ServiceTable table)
    {
        if (table == null) return;

        table.Release();
    }
}
