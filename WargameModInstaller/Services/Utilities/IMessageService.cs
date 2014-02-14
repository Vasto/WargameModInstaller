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

namespace WargameModInstaller.Services.Utilities
{   
    /// <summary>
    /// Interface for the message service.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Shows the specified message and returns the result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>The <see cref="MessageResult"/>.</returns>
        MessageResult Show(string message, string caption = "", MessageButton button = MessageButton.OK,
                           MessageImage icon = MessageImage.None);

        /// <summary>
        /// Shows the specified message and allows to await for the message to complete.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>A Task containing the <see cref="MessageResult"/>.</returns>
        System.Threading.Tasks.Task<MessageResult> ShowAsync(string message, string caption = "",
                                                             MessageButton button = MessageButton.OK,
                                                             MessageImage icon = MessageImage.None);
    }

    public enum MessageResult
    {
        /// <summary>
        /// No result available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Message is acknowledged.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Message is canceled.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Message is acknowledged with yes.
        /// </summary>
        Yes = 6,

        /// <summary>
        /// Message is acknowledged with no.
        /// </summary>
        No = 7
    }

    /// <summary>
    /// Available message buttons.
    /// </summary>
    public enum MessageButton
    {
        /// <summary>
        /// OK button.
        /// </summary>
        OK = 0,

        /// <summary>
        /// OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// Yes, No and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// Yes and No buttons.
        /// </summary>
        YesNo = 4,
    }

    /// <summary>
    /// Available message images.
    /// </summary>
    public enum MessageImage
    {
        /// <summary>
        /// Show no image.
        /// </summary>
        None = 0,

        /// <summary>
        /// Error image.
        /// </summary>
        Error = 16,

        /// <summary>
        /// Question image.
        /// </summary>
        Question = 32,

        /// <summary>
        /// Warning image.
        /// </summary>
        Warning = 48,

        /// <summary>
        /// Information image.
        /// </summary>
        Information = 64,
    }
 
}
