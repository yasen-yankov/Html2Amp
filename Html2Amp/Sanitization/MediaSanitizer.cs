﻿using AngleSharp.Dom;
using ComboRox.Core.Utilities.SimpleGuard;
using System;
using System.Text;

namespace Html2Amp.Sanitization
{
	public abstract class MediaSanitizer : Sanitizer
	{
		protected virtual bool ShoulRequestResourcesOnlyViaHttps
		{
			get
			{
				return false;
			}
		}

		protected virtual void SetMediaElementLayout(IElement element, IElement ampElement)
		{
			Guard.Requires(element, "element").IsNotNull();
			Guard.Requires(ampElement, "ampElement").IsNotNull();

			if (!ampElement.HasAttribute("layout"))
			{
				if (element.HasAttribute("height"))
				{
					if (element.HasAttribute("width"))
					{
						ampElement.SetAttribute("layout", "responsive");
					}
					else
					{
						ampElement.SetAttribute("layout", "fixed-height");
					}
				}
				else
				{
					ampElement.SetAttribute("layout", "fill");
				}
			}
		}

		protected virtual void RewriteSourceAttribute(IElement htmlElement)
		{
			if (!htmlElement.HasAttribute("src"))
			{
				return;
			}

			//Resources can be requested only via HTTPS
			var htmlElementSrc = new UriBuilder(htmlElement.GetAttribute("src"));

			if (htmlElementSrc.Scheme != "https")
			{
				var components = UriComponents.AbsoluteUri;
				if (htmlElementSrc.Uri.IsDefaultPort)
				{
					components &= ~UriComponents.Port;
				}

				htmlElementSrc.Scheme = "https";

				var newSourceAttributeValue = htmlElementSrc.Uri.GetComponents(components, UriFormat.UriEscaped).TrimEnd('/');
				htmlElement.SetAttribute("src", newSourceAttributeValue);
			}
		}

		protected override void SetElementLayout(IElement element, IElement ampElement)
		{
			base.SetElementLayout(element, ampElement);
			this.SetMediaElementLayout(element, ampElement);
		}

		protected virtual IElement SanitizeCore<T>(IDocument document, IElement htmlElement, string ampElementTagName)
			where T : IElement
		{
			Guard.Requires(document, "document").IsNotNull();
			Guard.Requires(htmlElement, "htmlElement").IsNotNull();
			Guard.Requires(ampElementTagName, "ampElementTagName").IsNotNullOrEmpty();

			var element = (T)htmlElement;

			if (this.ShoulRequestResourcesOnlyViaHttps)
			{
				this.RewriteSourceAttribute(element);
			}

			var ampElement = document.CreateElement(ampElementTagName);
			element.CopyTo(ampElement);

			this.SetElementLayout(element, ampElement);

            element.Parent.ReplaceChild(ampElement, element);

			return ampElement;
		}
	}
}