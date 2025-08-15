document.addEventListener('DOMContentLoaded', function () {
    // Set minimum date for check-in to today
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    const formatDate = (date) => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    };

    const checkinInput = document.getElementById('checkin-date');
    const checkoutInput = document.getElementById('checkout-date');

    if (checkinInput && checkoutInput) {
        checkinInput.min = formatDate(today);
        checkinInput.value = formatDate(today);

        checkoutInput.min = formatDate(tomorrow);
        checkoutInput.value = formatDate(tomorrow);

        // Update checkout min date when checkin changes
        checkinInput.addEventListener('change', function () {
            const newCheckinDate = new Date(this.value);
            const newMinCheckoutDate = new Date(newCheckinDate);
            newMinCheckoutDate.setDate(newMinCheckoutDate.getDate() + 1);

            checkoutInput.min = formatDate(newMinCheckoutDate);

            // If current checkout date is before new min date, update it
            const currentCheckoutDate = new Date(checkoutInput.value);
            if (currentCheckoutDate <= newCheckinDate) {
                checkoutInput.value = formatDate(newMinCheckoutDate);
            }

            updateBookingSummary();
        });

        checkoutInput.addEventListener('change', updateBookingSummary);

        // Initial calculation
        updateBookingSummary();
    }

    // Calculate and update booking summary
    function updateBookingSummary() {
        if (!checkinInput.value || !checkoutInput.value) return;

        const checkin = new Date(checkinInput.value);
        const checkout = new Date(checkoutInput.value);
        const diffTime = Math.abs(checkout - checkin);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        // Get base price from element
        const priceElement = document.querySelector('.current-price');
        if (!priceElement) return;

        const priceText = priceElement.textContent;
        const price = parseInt(priceText.replace(/[^\d]/g, ''));

        if (isNaN(price)) return;

        // Update nights count and calculations
        const nightsElement = document.getElementById('nights-count');
        const roomTotalElement = document.getElementById('room-total-price');
        const serviceFeeElement = document.getElementById('service-fee');
        const taxFeeElement = document.getElementById('tax-fee');
        const totalPriceElement = document.getElementById('total-price');

        if (nightsElement) nightsElement.textContent = diffDays;

        const roomTotal = price * diffDays;
        const serviceFee = Math.round(roomTotal * 0.1);
        const taxFee = Math.round(roomTotal * 0.08);
        const totalPrice = roomTotal + serviceFee + taxFee;

        if (roomTotalElement) roomTotalElement.textContent = formatCurrency(roomTotal);
        if (serviceFeeElement) serviceFeeElement.textContent = formatCurrency(serviceFee);
        if (taxFeeElement) taxFeeElement.textContent = formatCurrency(taxFee);
        if (totalPriceElement) totalPriceElement.textContent = formatCurrency(totalPrice);
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'decimal',
            maximumFractionDigits: 0
        }).format(value) + ' VNĐ';
    }

    // Handle booking form submission
    const bookingForm = document.getElementById('booking-form');
    if (bookingForm) {
        bookingForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const checkin = checkinInput.value;
            const checkout = checkoutInput.value;
            const guests = document.getElementById('guests').value;
            const roomId = new URLSearchParams(window.location.search).get('id');

            // Redirect to booking page with parameters
            window.location.href = `/Bookings/Create?roomId=${roomId}&checkin=${checkin}&checkout=${checkout}&guests=${guests}`;
        });
    }

    // Handle favorite button
    const favoriteBtn = document.querySelector('.favorite-btn');
    if (favoriteBtn) {
        favoriteBtn.addEventListener('click', function () {
            const icon = this.querySelector('i');

            if (icon.classList.contains('far')) {
                icon.classList.remove('far');
                icon.classList.add('fas');
                showToast('Đã thêm phòng vào danh sách yêu thích');
            } else {
                icon.classList.remove('fas');
                icon.classList.add('far');
                showToast('Đã xóa phòng khỏi danh sách yêu thích');
            }

            // Here you would send an AJAX request to update favorites
            const roomId = this.getAttribute('data-room-id');
            console.log('Toggle favorite for room:', roomId);
        });
    }

    // Handle share button
    const shareBtn = document.querySelector('.share-btn');
    if (shareBtn) {
        shareBtn.addEventListener('click', function () {
            const shareModal = new bootstrap.Modal(document.getElementById('shareModal'));
            shareModal.show();
        });
    }

    // Handle copy link
    const copyLinkBtn = document.getElementById('copy-link-btn');
    if (copyLinkBtn) {
        copyLinkBtn.addEventListener('click', function () {
            const shareLink = document.getElementById('share-link');
            shareLink.select();
            document.execCommand('copy');

            const originalText = this.textContent;
            this.textContent = 'Đã sao chép!';
            setTimeout(() => {
                this.textContent = originalText;
            }, 2000);
        });
    }

    // Toast notification helper
    function showToast(message, type = 'success') {
        // Create toast container if it doesn't exist
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastId = 'toast-' + Date.now();
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.id = toastId;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        // Add toast to container
        toastContainer.appendChild(toast);

        // Initialize and show toast
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();

        // Remove toast element after it's hidden
        toast.addEventListener('hidden.bs.toast', function () {
            toast.remove();
        });
    }
});