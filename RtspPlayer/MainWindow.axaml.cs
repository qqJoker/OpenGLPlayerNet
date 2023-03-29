using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RtspPlayer;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent(); 
        this.Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
    }
}