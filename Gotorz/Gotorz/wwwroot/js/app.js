// Add this function to your JavaScript file
window.downloadFile = function (filename, contentType, base64Data) {
    const link = document.createElement("a");
    link.href = `data:${contentType};base64,${base64Data}`;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
