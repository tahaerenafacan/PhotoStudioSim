using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServiceTableManager : MonoBehaviour
{
    [SerializeField] private List<ServiceTable> serviceTables = new();

    public bool HasAvailableTable => serviceTables.Any(table => table.IsAvailable);

    public ServiceTable TryReserveTable(CustomerController customer)
    {
        var availableTable = serviceTables.FirstOrDefault(table => table.IsAvailable);
        if (availableTable == null)
        {
            return null;
        }

        if (availableTable.TryReserve(customer))
        {
            return availableTable;
        }

        return null;
    }

    public void ReleaseTable(ServiceTable table)
    {
        if (table == null)
        {
            return;
        }

        table.Release();
    }
}
