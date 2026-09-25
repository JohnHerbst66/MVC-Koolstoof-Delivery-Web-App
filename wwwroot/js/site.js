// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Shrinks photos picked in <input type="file" data-resize-image> before upload. Phone
// photos are often 3-8 MB, but the host caps request bodies at 4.5 MB and the site only
// ever shows them a few hundred pixels wide. Falls back to the original file on any problem.
(function () {
    var MAX_SIDE = 1600;

    document.addEventListener('change', async function (event) {
        var input = event.target;
        if (!(input instanceof HTMLInputElement) || input.type !== 'file' || !input.hasAttribute('data-resize-image')) {
            return;
        }
        var file = input.files && input.files[0];
        if (!file || !file.type.startsWith('image/') || file.type === 'image/gif') {
            return;
        }

        try {
            var bitmap = await createImageBitmap(file);
            var scale = Math.min(1, MAX_SIDE / Math.max(bitmap.width, bitmap.height));
            var canvas = document.createElement('canvas');
            canvas.width = Math.round(bitmap.width * scale);
            canvas.height = Math.round(bitmap.height * scale);
            canvas.getContext('2d').drawImage(bitmap, 0, 0, canvas.width, canvas.height);

            var blob = await new Promise(function (resolve) { canvas.toBlob(resolve, 'image/jpeg', 0.82); });
            if (!blob || blob.size >= file.size) {
                return;
            }

            var resized = new File([blob], file.name.replace(/\.[^.]+$/, '') + '.jpg', { type: 'image/jpeg' });
            var transfer = new DataTransfer();
            transfer.items.add(resized);
            input.files = transfer.files;
        } catch (e) {
            // keep the original file
        }
    });
})();
