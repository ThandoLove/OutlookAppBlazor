let loginDialogComponent;
let blazorComponentInstanceReference = null;

window.officeBridge = {
    // STABILIZATION REFACTOR: Registers the component reference to resolve the lost callback trap
    registerMailContextHandler: function (dotNetReference) {
        blazorComponentInstanceReference = dotNetReference;
        console.log("[OfficeJS] Successfully synchronized Blazor component instance hook.");

        // Securely trigger initial selection context processing on boot
        window.officeBridge.processActiveMailSelection();
    },
    initializeEventHandlers: function () {
        if (typeof Office === 'undefined' || !Office.context || !Office.context.mailbox) {
            console.warn("OfficeJS framework context is unavailable or running outside an active Outlook host.");
            return;
        }

        // Section 10 Resolution: Listens to selection switches natively
        Office.context.mailbox.addHandlerAsync(Office.EventType.ItemChanged, function (eventArgs) {
            console.log("[OfficeJS] Intercepted mail item selection change event.");
            window.officeBridge.processActiveMailSelection();
        });
    },

    processActiveMailSelection: function () {
        if (typeof Office === 'undefined' || !Office.context || !Office.context.mailbox) return;

        const currentItem = Office.context.mailbox.item;
        if (currentItem && blazorComponentInstanceReference) {
            const mailPayload = {
                senderEmail: currentItem.sender ? currentItem.sender.emailAddress : "",
                senderName: currentItem.sender ? currentItem.sender.displayName : "",
                messageId: currentItem.itemId || ""
            };

            // FIX: Proxy calls safely through the active instance tracker instead of a broken static namespace
            blazorComponentInstanceReference.invokeMethodAsync('NotifyMailItemSelectionChanged', mailPayload)
                .catch(err => console.error("[OfficeJS] Failed to dispatch context parameters to Blazor instance:", err));
        }
    },
    attachFileToActiveEmail: function (fileUrl, fileName) {
        if (!Office.context.mailbox || !Office.context.mailbox.item) {
            alert("This action requires an active email compilation state framework.");
            return;
        }

        Office.context.mailbox.item.addItemAttachmentAsync(
            fileUrl,
            fileName,
            { isInline: false },
            function (asyncResult) {
                if (asyncResult.status === Office.AsyncResultStatus.Failed) {
                    alert("OfficeJS Attachment rejection error: " + asyncResult.error.message);
                } else {
                    alert("Success! Attached " + fileName + " straight to email composition frame.");
                }
            }
        );
    },
    openSecureLoginWindow: function (targetLoginUrl) {
        Office.context.ui.displayDialogAsync(targetLoginUrl, { height: 60, width: 40, displayInIframe: false },
            function (asyncResult) {
                if (asyncResult.status === Office.AsyncResultStatus.Failed) {
                    console.error("Failed to generate authentication popup container frame: " + asyncResult.error.message);
                    return;
                }

                loginDialogComponent = asyncResult.value;

                loginDialogComponent.addEventHandler(Office.EventType.DialogMessageReceived, function (arg) {
                    const payload = JSON.parse(arg.message);
                    loginDialogComponent.close();

                    if (payload.status === "success" && blazorComponentInstanceReference) {
                        // FIX: Redirect callbacks safely through the verified runtime execution handles
                        blazorComponentInstanceReference.invokeMethodAsync('ProcessTokenExchangeCallback', payload.authCode)
                            .catch(err => console.error("[OfficeJS] Token callback routing error:", err));
                    }
                });
            }
        );
    }
};

if (typeof Office !== 'undefined') {
    Office.onReady(function (info) {
        if (info.host === Office.HostType.Outlook) {
            window.officeBridge.initializeEventHandlers();
        }
    });
}
