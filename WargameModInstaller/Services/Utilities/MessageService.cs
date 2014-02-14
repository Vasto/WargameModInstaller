//The MIT License

//Copyright (c) 2013 Thomas Ibel

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WargameModInstaller.Services.Utilities
{

    /// <summary>
    /// Message service that implements the <see cref="IMessageService"/> by using the <see cref="MessageBox"/> class.
    /// </summary>
    public class MessageService : IMessageService
    {
        static MessageResult TranslateMessageBoxResult(MessageBoxResult result)
        {
            var value = result.ToString();
            return (MessageResult)Enum.Parse(typeof(MessageResult), value, true);
        }

        static MessageBoxImage TranslateMessageImage(MessageImage image) {
            var value = image.ToString();
            return (MessageBoxImage)Enum.Parse(typeof(MessageBoxImage), value, true);
        }

        static Window GetActiveWindow() {
            if (Application.Current == null) {
                return null;
            }

            var active = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            return active ?? Application.Current.MainWindow;
        }

        static MessageBoxButton TranslateMessageButton(MessageButton button)
        {
            try
            {
                var value = button.ToString();
                return (MessageBoxButton)Enum.Parse(typeof(MessageBoxButton), value, true);
            }
            catch (Exception)
            {
                throw new NotSupportedException(string.Format("Unfortunately, the default MessageBox class of does not support '{0}' button.", button));
            }
        }

        /// <summary>
        /// Shows the specified message and returns the result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>The <see cref="MessageResult"/>.</returns>
        public MessageResult Show(string message, string caption = "", MessageButton button = MessageButton.OK, MessageImage icon = MessageImage.None)
        {
            return ShowMessageBox(message, caption, button, icon);
        }

        /// <summary>
        /// Shows the specified message and allows to await for the message to complete.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>A Task containing the <see cref="MessageResult"/>.</returns>
        public Task<MessageResult> ShowAsync(string message, string caption = "", MessageButton button = MessageButton.OK, MessageImage icon = MessageImage.None)
        {
            var taskSource = new TaskCompletionSource<MessageResult>();
            try
            {
                var result = ShowMessageBox(message, caption, button, icon);
                taskSource.SetResult(result);
            }
            catch (Exception ex)
            {
                taskSource.SetException(ex);
            }
            return taskSource.Task;
        }

        static MessageResult ShowMessageBox(string message, string caption, MessageButton button, MessageImage icon)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            var result = MessageBoxResult.None;
            var messageBoxButton = TranslateMessageButton(button);

            var messageBoxImage = TranslateMessageImage(icon);

            var activeWindow = GetActiveWindow();
            if (activeWindow != null) {
                result = MessageBox.Show(activeWindow, message, caption, messageBoxButton, messageBoxImage);
            }
            else {
                result = MessageBox.Show(message, caption, messageBoxButton, messageBoxImage);
            }

            return TranslateMessageBoxResult(result);
        }
    }
}
