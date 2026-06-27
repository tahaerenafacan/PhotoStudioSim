using System;
using System.Collections.Generic;
using System.Linq;
using Evo.UI;
using SyntaxSultan.PrinterSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class PrintPopup : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.RawImage printingImagePrewiev;
    [SerializeField] private Dropdown printerDropdown;
    [SerializeField] private Selector paperSizeSelector;
    [SerializeField] private Dropdown orientationDropdown;
    [SerializeField] private Dropdown fitDropdown;
    [SerializeField] private Switch coloredSwitch;
    [SerializeField] private Dropdown qualityDropdown;

    [Header("Ink")]
    [SerializeField] private ProgressBar cyanInkAmount;
    [SerializeField] private ProgressBar magentaInkAmount;
    [SerializeField] private ProgressBar yellowInkAmount;
    [SerializeField] private ProgressBar keyInkAmount;
    
    [Header("Paper Tray")]
    [SerializeField] private TextMeshProUGUI remainPaperText;
    [SerializeField] private TextMeshProUGUI maxPaperText;
    [SerializeField] private ProgressBar remainingPaperProgressBar;

    [SerializeField] private QuantitySelector quantitySelector;
    [SerializeField] private Button printButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Localization")]
    [SerializeField] private LocalizedString printerNotFound;
    
    public event Action<PrintSettings> OnPrintButtonClicked;
    
    private ItemPaperPrinter currentPrinter;
    private List<INetworkDevice> availablePrinters = new List<INetworkDevice>();
    private List<PrintQuality> availableQualities = new();

    private void Awake()
    {
        PopulateSelectorItems(paperSizeSelector, typeof(PrintPaperSize));
        PopulateDropdownItems(orientationDropdown, typeof(PrintPaperOrientation));
        PopulateDropdownItems(fitDropdown, typeof(PrintPaperFit));
        
        printButton.onClick.AddListener(PrintButtonClicked);
        cancelButton.onClick.AddListener(ClosePrintPopup);
        printerDropdown.onItemSelected.AddListener(OnPrinterDropdownChanged);
    }

    private void OnDestroy()
    {
        printButton.onClick.RemoveListener(PrintButtonClicked);
        cancelButton.onClick.RemoveListener(ClosePrintPopup);
        printerDropdown.onItemSelected.RemoveListener(OnPrinterDropdownChanged);
    }

    private void Start()
    {
        RefreshPrinterList(null, null);
        if (Router.Instance != null)
        {
            Router.Instance.OnNetworkDevicesChanged += RefreshPrinterList;
        }
    }
    
    private void RefreshPrinterList(object sender, NetworkDevicesChangedEventArgs networkDevicesChangedEventArgs)
    {
        availablePrinters = Router.Instance.GetDevicesByType(NetworkDeviceType.Printer);
        printerDropdown.ClearAllItems();

        if (availablePrinters.Count == 0)
        {
            printerDropdown.AddItem(printerNotFound.GetLocalizedString());
            printButton.interactable = false;
            
            // Clear UI tracking if no printers exist
            if (currentPrinter != null)
            {
                currentPrinter.InkSystem.OnInkChanged -= UpdateInkUI;
                currentPrinter.PaperTray.OnPaperCountChanged -= UpdatePaperUI;
                currentPrinter = null;
            }
            return;
        }

        printButton.interactable = true;
        string[] printerNames = availablePrinters.Select(p => p.GetNetworkDeviceData().NetworkDeviceName).ToArray();
        printerDropdown.AddItems(printerNames);
        RefreshQualityDropdown(); 
        
        // Ensure UI bars sync with the automatically selected first item
        OnPrinterDropdownChanged(0);
    }
    
    private void RefreshQualityDropdown()
    {
        PrintQuality maxQuality = PrintQuality.UltraHigh;

        if (availablePrinters.Count > 0 && printerDropdown.selectedIndex >= 0 && printerDropdown.selectedIndex < availablePrinters.Count)
        {
            if (availablePrinters[printerDropdown.selectedIndex] is ItemPaperPrinter printer)
                maxQuality = printer.MaxSupportedQuality;
        }

        availableQualities.Clear();
        qualityDropdown.ClearAllItems();
    
        var options = new List<string>();

        foreach (PrintQuality quality in Enum.GetValues(typeof(PrintQuality)))
        {
            if (quality <= maxQuality)
            {
                availableQualities.Add(quality);
                options.Add(GetQualityDisplayName(quality));
            }
        }

        qualityDropdown.AddItems(options.ToArray());
    
        if (options.Count > 0)
        {
            qualityDropdown.selectedIndex = options.Count - 1;
        }
    }

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
        OnPrintButtonClicked?.Invoke(GeneratePrintSettings());
        ClosePrintPopup();
    }
    
    private void ClosePrintPopup()
    {
        gameObject.SetActive(false);
    }

    private PrintSettings GeneratePrintSettings()
    {
        INetworkDevice selectedPrinter = null;
        if (availablePrinters.Count > 0)
        {
            selectedPrinter = availablePrinters[Mathf.Clamp(printerDropdown.selectedIndex, 0, availablePrinters.Count - 1)];
        }
        
        PrintQuality selectedQuality = availableQualities.Count > 0
            ? availableQualities[Mathf.Clamp(qualityDropdown.selectedIndex, 0, availableQualities.Count - 1)]
            : PrintQuality.Low;
        
        return new PrintSettings 
        {
            targetPrinter = selectedPrinter,
            paperSize = ParseSelectorValue<PrintPaperSize>(paperSizeSelector),
            paperOrientation = ParseDropdownValue<PrintPaperOrientation>(orientationDropdown),
            paperFit = ParseDropdownValue<PrintPaperFit>(fitDropdown),
            isColored        = coloredSwitch.IsOn, 
            quality          = selectedQuality,
            quantity = quantitySelector.CurrentQuantity
        };
    }

    private void OnPrinterDropdownChanged(int index)
    {
        if (currentPrinter != null)
        {
            currentPrinter.InkSystem.OnInkChanged -= UpdateInkUI;
            currentPrinter.PaperTray.OnPaperCountChanged -= UpdatePaperUI;
            currentPrinter = null;
        }

        // Bug Fix: Safely fetch the selected printer from the list
        if (index < 0 || index >= availablePrinters.Count) return;
        INetworkDevice selectedPrinter = availablePrinters[index];

        currentPrinter = Router.Instance.GetNetworkDeviceComponent<ItemPaperPrinter>(selectedPrinter);
    
        if (currentPrinter != null)
        {
            currentPrinter.InkSystem.OnInkChanged += UpdateInkUI;
            currentPrinter.PaperTray.OnPaperCountChanged += UpdatePaperUI;
        
            UpdateInkUI();
            UpdatePaperUI();
            RefreshQualityDropdown();
        }
    }

    private void UpdateInkUI()
    {
        if (currentPrinter == null) return;
    
        var ink = currentPrinter.InkSystem;
        cyanInkAmount.SetValue((ink.Cyan / ink.MaxInkLevel)*100);
        magentaInkAmount.SetValue(ink.Magenta / ink.MaxInkLevel*100);
        yellowInkAmount.SetValue(ink.Yellow / ink.MaxInkLevel*100);
        keyInkAmount.SetValue(ink.Black / ink.MaxInkLevel*100);
    }

    private void UpdatePaperUI()
    {
        if (currentPrinter == null) return;
    
        var tray = currentPrinter.PaperTray;
        remainPaperText.text = tray.CurrentCount.ToString();
        maxPaperText.text = tray.MaxCapacity.ToString();
        remainingPaperProgressBar.SetValue(tray.NormalizedFillRatio*100);
    }

    // --------Helpers---------
    
    private T ParseDropdownValue<T>(Evo.UI.Dropdown dropdown) where T : struct
    {
        string enumName = Enum.GetNames(typeof(T))[dropdown.selectedIndex];
        return Enum.TryParse(enumName, out T result) ? result : default;
    }

    private T ParseSelectorValue<T>(Evo.UI.Selector dropdown) where T : struct
    {
        string enumName = Enum.GetNames(typeof(T))[dropdown.selectedIndex];
        return Enum.TryParse(enumName, out T result) ? result : default;
    }

    private void PopulateDropdownItems(Evo.UI.Dropdown dropdown, Type enumType)
    {
        dropdown.ClearAllItems();
        string[] enumNames = Enum.GetNames(enumType);
        dropdown.AddItems(enumNames);
    }

    private void PopulateSelectorItems(Evo.UI.Selector selector, Type enumType)
    {
        selector.ClearItems();
        string[] enumNames = Enum.GetNames(enumType);
        selector.AddItems(enumNames);
    }
}