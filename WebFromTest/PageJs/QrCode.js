$(function() {
  //reader
  const formatsToSupport = [
    Html5QrcodeSupportedFormats.QR_CODE,
    Html5QrcodeSupportedFormats.CODE_39,
    Html5QrcodeSupportedFormats.CODE_93,
    Html5QrcodeSupportedFormats.CODE_128,
    Html5QrcodeSupportedFormats.EAN_13,
    Html5QrcodeSupportedFormats.EAN_8
  ];
  var html5QrcodeScanner = new Html5QrcodeScanner("reader", {
    fps: 10,
    qrbox: 250,
    formatsToSupport: formatsToSupport
  });
  html5QrcodeScanner.render(
    function onScanSuccess(decodedText, decodedResult) {
      // Handle on success condition with the decoded text or result.
      console.log("Scan result:" + decodedText, decodedResult);
    },
    function onScanError(errorMessage) {
      // handle on error condition, with error message
      //console.log('Scan result: ${decodedText}', errorMessage);
    }
  );

  //own reader
  const html5QrCode = new Html5Qrcode(/* element id */ "pic");
  Html5Qrcode.getCameras()
    .then(function(devices) {
      /**
       * devices would be an array of objects of type:
       * { id: "id", label: "label" }
       */
      if (devices && devices.length != 0) {
        $.each(devices, function(i, e) {
          $("#cameraList").append(
            '<option value="' + e.id + '">' + e.label + "</option>"
          );
        });
      }
    })
    .catch(function(err) {
      // handle err
    });

  $("#btnScanQRCode").on("click", function (e) {
    var cameraId = $("#cameraList").val();
    html5QrCode
      .start(
        cameraId,
        {
          fps: 10, // Optional, frame per seconds for qr code scanning
          //qrbox: 200 , // Optional, if you want bounded box UI
          formatsToSupport: formatsToSupport
        },
        function onScanSuccess(decodedText, decodedResult) {
          // Handle on success condition with the decoded text or result.
          $("#qrCodeText").text(
            decodedText + ";" + JSON.stringify(decodedResult)
          );
          html5QrCode.stop();
          $("#pic").empty();
        },
        function onScanError(errorMessage) {
          // handle on error condition, with error message
          $("#qrCodeText").text(errorMessage);
        }
      )
      .catch(function(err) {
        // failure, handle it.
        $("#qrCodeText").text(err);
      });
  });

  $("#btnStopScan").on("click", function(e) {
    html5QrCode.stop();
    $("#pic").empty();
  });
  // File based scanning
  $("#qr-input-file").on("change", function(e) {
    if (e.target.files.length == 0) {
      // No file selected, ignore
      return;
    }
    const imageFile = e.target.files[0];
    // Scan QR Code
    html5QrCode
      .scanFile(imageFile, true)
      .then(function(decodedText, decodedResult) {
        // success, use decodedText
        $("#qrCodeText").text(
          decodedText + ";" + JSON.stringify(decodedResult)
        );
      })
      .catch(function(err) {
        // failure, handle it.
        $("#qrCodeText").text(err);
      });
  });
});
