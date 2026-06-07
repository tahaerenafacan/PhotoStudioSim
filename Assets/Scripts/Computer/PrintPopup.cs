using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrintPopup : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown printerSelectDropdown;
    [SerializeField] private TMP_Dropdown paperSizeSelectDropdown;
    [SerializeField] private TMP_Dropdown orientationSelectDropdown;
    [SerializeField] private TMP_Dropdown fitSelectDropdown;
    [SerializeField] private TMP_Dropdown colorModeDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private RawImage printingImagePrewiev;
    [SerializeField] private Button printButton;
    [SerializeField] private Button cancelButton;
    
    public event Action<PrintSettings> OnPrint;
    
    private List<INetworkDevice> availablePrinters = new List<INetworkDevice>();
    private List<PrintQuality> availableQualities = new();

    private void Awake()
    {
        PopulateDropdownItems(paperSizeSelectDropdown, typeof(PrintPaperSize));
        PopulateDropdownItems(orientationSelectDropdown, typeof(PrintPaperOrientation));
        PopulateDropdownItems(fitSelectDropdown, typeof(PrintPaperFit));
        
        printButton.onClick.AddListener(PrintButtonClicked);
        cancelButton.onClick.AddListener(ClosePrintPopup);
    }

    private void OnEnable()
    {
        RefreshPrinterList();
    }
    
    private void RefreshPrinterList()
    {
        availablePrinters = Router.Instance.GetDevicesByType(NetworkDeviceType.Printer);
        printerSelectDropdown.ClearOptions();

        if (availablePrinters.Count == 0)
        {
            printerSelectDropdown.AddOptions(new List<string> { "Yazıcı Bulunamadı" });
            printButton.interactable = false;
            return;
        }

        printButton.interactable = true;
        List<string> printerNames = availablePrinters.Select(p => p.GetNetworkDeviceData().NetworkDeviceName).ToList();
        printerSelectDropdown.AddOptions(printerNames);
        RefreshQualityDropdown(); 
    }
    
    /// <summary>
    /// Seçili yazıcının MaxSupportedQuality'sine göre quality dropdown'ını filtreler.
    /// Yazıcı değiştiğinde veya liste yenilendiğinde çağrılmalı.
    /// </summary>
    private void RefreshQualityDropdown()
    {
        PrintQuality maxQuality = PrintQuality.UltraHigh;

        if (availablePrinters.Count > 0 && printerSelectDropdown.value < availablePrinters.Count)
        {
            if (availablePrinters[printerSelectDropdown.value] is ItemPaperPrinter printer)
                maxQuality = printer.GetMaxSupportedQuality();
        }

        availableQualities.Clear();
        qualityDropdown.ClearOptions();
        var options = new List<string>();

        foreach (PrintQuality quality in Enum.GetValues(typeof(PrintQuality)))
        {
            if (quality <= maxQuality)
            {
                availableQualities.Add(quality);
                options.Add(GetQualityDisplayName(quality));
            }
        }

        qualityDropdown.AddOptions(options);
        // Mevcut seçimi en yüksekte bırak
        qualityDropdown.value = options.Count - 1;
    }

    private void OnPrinterSelectionChanged(int _) => RefreshQualityDropdown();

    private string GetQualityDisplayName(PrintQuality q) => q switch
    {
        PrintQuality.Low      => "Düşük Kalite",
        PrintQuality.Average  => "Ortalama Kalite",
        PrintQuality.High     => "Yüksek Kalite",
        PrintQuality.UltraHigh => "Ultra Yüksek Kalite",
        _                     => q.ToString()
    };

    public void SetPreviewImage(Texture2D texture)
    {
        printingImagePrewiev.texture = texture;
    }

    private void PrintButtonClicked()
    {
        OnPrint?.Invoke(GeneratePrintSettings());
        ClosePrintPopup();
    }

    private PrintSettings GeneratePrintSettings()
    {
        INetworkDevice selectedPrinter = null;
        if (availablePrinters.Count > 0)
        {
            selectedPrinter = availablePrinters[printerSelectDropdown.value];
        }
        
        PrintQuality selectedQuality = availableQualities.Count > 0
            ? availableQualities[Mathf.Clamp(qualityDropdown.value, 0, availableQualities.Count - 1)]
            : PrintQuality.Low;
        
        return new PrintSettings 
        {
            targetPrinter = selectedPrinter,
            paperSize = ParseDropdownValue<PrintPaperSize>(paperSizeSelectDropdown),
            paperOrientation = ParseDropdownValue<PrintPaperOrientation>(orientationSelectDropdown),
            paperFit = ParseDropdownValue<PrintPaperFit>(fitSelectDropdown),
            isColored        = colorModeDropdown.value == 1, // 0=SB, 1=Renkli
            quality          = selectedQuality
        };
    }

    // --------Helpers---------
    
    private T ParseDropdownValue<T>(TMPro.TMP_Dropdown dropdown) where T : struct
    {
        if (Enum.TryParse(dropdown.value.ToString(), out T result))
        {
            return result;
        }
        return default;
    }
    
    private void ClosePrintPopup()
    {
        gameObject.SetActive(false);
    }

    private void PopulateDropdownItems(TMP_Dropdown dropdown, Type enumType)
    {
        dropdown.ClearOptions();
        string[] enumNames = Enum.GetNames(enumType);
        List<string> options = new List<string>(enumNames);
        dropdown.AddOptions(options);
    }
}
