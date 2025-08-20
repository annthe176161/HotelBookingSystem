document.addEventListener("DOMContentLoaded", function () {
  const checkinInput = document.querySelector('input[name="CheckInDate"]');
  const checkoutInput = document.querySelector('input[name="CheckOutDate"]');
  const guestCountSelect = document.querySelector('select[name="GuestCount"]');
  const termsCheckbox = document.getElementById("terms");
  const submitBtn = document.getElementById("submitBtn");

  // Get room price from hidden input
  const roomPriceInput = document.querySelector('input[name="RoomPrice"]');
  let roomPrice = 0;

  if (roomPriceInput) {
    roomPrice = parseFloat(roomPriceInput.value) || 0;
  } else {
    // Fallback: extract from display text
    const priceElements = document.querySelectorAll(".detail-row span");
    for (let element of priceElements) {
      if (
        element.textContent.includes("VNĐ") &&
        element.textContent.includes("/đêm")
      ) {
        const match = element.textContent.match(/[\d,]+/);
        if (match) {
          roomPrice = parseFloat(match[0].replace(/,/g, ""));
          break;
        }
      }
    }
  }

  console.log("Room price:", roomPrice); // Debug

  function updateSummary() {
    const checkin = checkinInput?.value;
    const checkout = checkoutInput?.value;
    const guests = guestCountSelect?.value;

    // Update display elements
    const displayCheckin = document.getElementById("display-checkin");
    const displayCheckout = document.getElementById("display-checkout");
    const displayGuests = document.getElementById("display-guests");
    const displayNights = document.getElementById("display-nights");
    const displayTotal = document.getElementById("display-total");

    // Format and display dates
    if (displayCheckin) {
      displayCheckin.textContent = checkin
        ? new Date(checkin).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
          })
        : "Chưa chọn";
    }

    if (displayCheckout) {
      displayCheckout.textContent = checkout
        ? new Date(checkout).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
          })
        : "Chưa chọn";
    }

    if (displayGuests) {
      displayGuests.textContent = guests ? guests + " khách" : "Chưa chọn";
    }

    // Calculate nights and total
    if (checkin && checkout) {
      const checkinDate = new Date(checkin);
      const checkoutDate = new Date(checkout);
      const nights = Math.max(
        0,
        Math.ceil((checkoutDate - checkinDate) / (1000 * 60 * 60 * 24))
      );
      const total = nights * roomPrice;

      if (displayNights) {
        displayNights.textContent = nights + " đêm";
      }

      if (displayTotal) {
        displayTotal.textContent = total.toLocaleString("vi-VN") + " VNĐ";
      }

      console.log("Nights:", nights, "Total:", total); // Debug
    } else {
      if (displayNights) displayNights.textContent = "0 đêm";
      if (displayTotal) displayTotal.textContent = "0 VNĐ";
    }

    // Update submit button state
    updateSubmitButton();
  }

  function updateSubmitButton() {
    if (submitBtn && termsCheckbox) {
      submitBtn.disabled = !termsCheckbox.checked;

      if (termsCheckbox.checked) {
        submitBtn.classList.remove("btn-secondary");
        submitBtn.classList.add("btn-primary");
      } else {
        submitBtn.classList.remove("btn-primary");
        submitBtn.classList.add("btn-secondary");
      }
    }
  }

  // Event listeners
  if (checkinInput) {
    checkinInput.addEventListener("change", updateSummary);
  }

  if (checkoutInput) {
    checkoutInput.addEventListener("change", updateSummary);
  }

  if (guestCountSelect) {
    guestCountSelect.addEventListener("change", updateSummary);
  }

  if (termsCheckbox) {
    termsCheckbox.addEventListener("change", updateSubmitButton);
  }

  // Set minimum dates
  const today = new Date().toISOString().split("T")[0];
  if (checkinInput) {
    checkinInput.min = today;
  }

  // Update checkout minimum date when checkin changes
  if (checkinInput && checkoutInput) {
    checkinInput.addEventListener("change", function () {
      const selectedDate = new Date(this.value);
      selectedDate.setDate(selectedDate.getDate() + 1); // Minimum 1 night
      checkoutInput.min = selectedDate.toISOString().split("T")[0];

      if (checkoutInput.value && checkoutInput.value <= this.value) {
        checkoutInput.value = "";
        updateSummary();
      }
    });
  }

  // Form validation
  const form = document.querySelector("form");
  if (form) {
    form.addEventListener("submit", function (e) {
      if (!termsCheckbox || !termsCheckbox.checked) {
        e.preventDefault();
        alert("Vui lòng đồng ý với điều khoản và điều kiện");
        if (termsCheckbox) termsCheckbox.focus();
        return false;
      }

      if (!checkinInput?.value) {
        e.preventDefault();
        alert("Vui lòng chọn ngày nhận phòng");
        checkinInput.focus();
        return false;
      }

      if (!checkoutInput?.value) {
        e.preventDefault();
        alert("Vui lòng chọn ngày trả phòng");
        checkoutInput.focus();
        return false;
      }

      if (!guestCountSelect?.value) {
        e.preventDefault();
        alert("Vui lòng chọn số khách");
        guestCountSelect.focus();
        return false;
      }
    });
  }

  // Initial setup
  updateSummary();
  updateSubmitButton();
});
