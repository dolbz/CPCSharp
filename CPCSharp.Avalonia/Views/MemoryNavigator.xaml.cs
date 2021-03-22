//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CPCSharp.App.Views
{
    public class MemoryNavigator : Window
    {
        public MemoryNavigator()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}