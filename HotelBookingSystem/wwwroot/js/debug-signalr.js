// Debug SignalR Connection
window.addEventListener("load", function () {
  console.log("[DEBUG] Page loaded, checking SignalR...");

  // Add debug button
  if (window.isUserAuthenticated) {
    const debugDiv = document.createElement("div");
    debugDiv.style.cssText =
      "position: fixed; top: 10px; right: 10px; background: white; padding: 10px; border: 1px solid #ccc; border-radius: 5px; z-index: 9999; font-size: 12px;";
    debugDiv.innerHTML = `
            <h6>SignalR Debug</h6>
            <div>Status: <span id="debug-status">Checking...</span></div>
            <div>User: <span id="debug-user">${
              window.currentUserId || "N/A"
            }</span></div>
            <div>Admin: <span id="debug-admin">${
              window.isUserAdmin || "N/A"
            }</span></div>
            <button onclick="debugSignalR()" style="margin-top: 5px; padding: 3px 6px; font-size: 11px;">Check</button>
            <button onclick="testAdminNotification()" style="margin-top: 5px; padding: 3px 6px; font-size: 11px;">Test Admin</button>
        `;
    document.body.appendChild(debugDiv);
  }
});

function debugSignalR() {
  console.log("[DEBUG] Debugging SignalR...");

  // Check if connection exists
  if (typeof signalR !== "undefined") {
    console.log("[DEBUG] SignalR library loaded");
    document.getElementById("debug-status").textContent = "Library OK";
  } else {
    console.log("[DEBUG] SignalR library NOT loaded");
    document.getElementById("debug-status").textContent = "Library Missing";
    return;
  }

  // Check connection state (từ signalr-notifications.js)
  setTimeout(() => {
    const connections = [];
    for (let prop in window) {
      if (prop.includes("connection") || prop.includes("signalr")) {
        connections.push(prop);
      }
    }
    console.log("[DEBUG] Found connection properties:", connections);
  }, 1000);
}

function testAdminNotification() {
  console.log("[DEBUG] Testing admin notification via controller...");

  fetch("/Test/TestAdminNotification", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken:
        document.querySelector('input[name="__RequestVerificationToken"]')
          ?.value || "",
    },
  })
    .then((response) => {
      console.log("[DEBUG] Response status:", response.status);
      console.log("[DEBUG] Response ok:", response.ok);

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return response.text(); // Get text first to check content
    })
    .then((text) => {
      console.log("[DEBUG] Response text:", text);

      try {
        const data = JSON.parse(text);
        console.log("[DEBUG] Test response:", data);
        alert("Test sent: " + JSON.stringify(data));
      } catch (e) {
        console.error("[DEBUG] JSON parse error:", e);
        alert("Response received but not JSON: " + text);
      }
    })
    .catch((error) => {
      console.error("[DEBUG] Test error:", error);
      alert("Test error: " + error.message);
    });
}
