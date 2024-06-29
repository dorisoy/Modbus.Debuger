﻿namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class AnalyzeFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    #region **************************************** 构造函数 ****************************************
    public AnalyzeFrameViewModel() { }
    public AnalyzeFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ExecuteCommand = new(Execute);
        ModbusByteOrderChangedCommand = new DelegateCommand<ModbusByteOrder?>(ModbusByteOrderChanged);
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters != null && parameters.ContainsKey("ModbusByteOrder"))
        {
            ModbusByteOrder = parameters.GetValue<ModbusByteOrder>("ModbusByteOrder");
        }
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            ModbusRtuFrame = parameters.GetValue<ModbusRtuFrame>("Value");
            if (ModbusRtuFrame.RegisterValues?.Length > 0)
            {
                ModbusRtuDatas.AddRange(Enumerable.Range(0, ModbusRtuFrame.RegisterValues.Length / 2).Select(x => new ModbusRtuData()));

            }

            //将读取的数据写入
            for (int i = 0; i < ModbusRtuDatas.Count; i++)
            {
                ModbusRtuDatas[i].Location = i * 2;         //在源字节数组中的起始位置 源字节数组为完整的数据帧,帧头部分3字节 每个值为1个word2字节
                ModbusRtuDatas[i].ModbusByteOrder = ModbusByteOrder; //字节序
                ModbusRtuDatas[i].OriginValue = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(ModbusRtuFrame.RegisterValues, 2 * i);
                ModbusRtuDatas[i].OriginBytes = ModbusRtuFrame.RegisterValues;        //源字节数组
            }

        }
    }
    #endregion

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// ModbusRtuFrame
    /// </summary>
    public ModbusRtuFrame ModbusRtuFrame { get => _ModbusRtuFrame; set => SetProperty(ref _ModbusRtuFrame, value); }
    private ModbusRtuFrame _ModbusRtuFrame;

    /// <summary>
    /// 字节序
    /// </summary>
    public ModbusByteOrder ModbusByteOrder { get => _ModbusByteOrder; set => SetProperty(ref _ModbusByteOrder, value); }
    private ModbusByteOrder _ModbusByteOrder = ModbusByteOrder.DCBA;

    /// <summary>
    /// ModbusRtu的寄存器值
    /// </summary>
    public ObservableCollection<ModbusRtuData> ModbusRtuDatas { get => _ModbusRtuDatas; set => SetProperty(ref _ModbusRtuDatas, value); }
    private ObservableCollection<ModbusRtuData> _ModbusRtuDatas = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }

    /// <summary>
    /// 字节序切换
    /// </summary>
    public DelegateCommand<ModbusByteOrder?> ModbusByteOrderChangedCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new DialogParameters();
        param.Add("Value", CurrentDto);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (DialogHost.IsDialogOpen(DialogHostName))
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

    /// <summary>
    /// 弹窗
    /// </summary>
    private void OpenDialogView()
    {
        
    }

    /// <summary>
    /// 字节序切换
    /// </summary>
    /// <param name="order"></param>
    private void ModbusByteOrderChanged(ModbusByteOrder? order)
    {
        try
        {
            if (order == null)
            {
                return;
            }


            foreach (var item in ModbusRtuDatas)
            {
                item.ModbusByteOrder = ModbusByteOrder;
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
    #endregion
}
