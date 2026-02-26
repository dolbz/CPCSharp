// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CPCSharp.ViewModels;

namespace CPCSharp
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            var name = data?.GetType().FullName?.Replace("ViewModel", "View") ?? string.Empty;
            var type = Type.GetType(name);

            if (type is not null)
            {
                var instance = Activator.CreateInstance(type) as Control;

                if (instance is not null)
                {
                    return instance;
                }
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}