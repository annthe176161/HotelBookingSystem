document.addEventListener('DOMContentLoaded', function () {
    // Date inputs validation
    const checkinInput = document.getElementById('checkin');
    const checkoutInput = document.getElementById('checkout');

    if (checkinInput && checkoutInput) {
        const today = new Date();
        const tomorrow = new Date(today);
        tomorrow.setDate(tomorrow.getDate() + 1);

        const formatDate = (date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        // Set min dates if not already set
        if (!checkinInput.value) {
            checkinInput.value = formatDate(today);
        }
        if (!checkoutInput.value) {
            checkoutInput.value = formatDate(tomorrow);
        }

        checkinInput.min = formatDate(today);

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
        });
    }

    // Initialize price range slider
    const priceRangeSlider = document.getElementById('priceRange');

    if (priceRangeSlider) {
        noUiSlider.create(priceRangeSlider, {
            start: [0, 10000000],
            connect: true,
            step: 100000,
            range: {
                'min': 0,
                'max': 10000000
            }
        });

        const priceMin = document.getElementById('priceMin');
        const priceMax = document.getElementById('priceMax');
        const minPriceInput = document.getElementById('minPrice');
        const maxPriceInput = document.getElementById('maxPrice');

        priceRangeSlider.noUiSlider.on('update', function (values, handle) {
            const value = Math.round(values[handle]);

            if (handle === 0) {
                priceMin.textContent = formatCurrency(value);
                minPriceInput.value = value;
            } else {
                priceMax.textContent = formatCurrency(value);
                maxPriceInput.value = value;
            }
        });

        function formatCurrency(value) {
            return new Intl.NumberFormat('vi-VN', {
                style: 'decimal',
                maximumFractionDigits: 0
            }).format(value);
        }
    }

    // Mobile filters toggle
    const showFiltersBtn = document.getElementById('showFilters');
    const closeFiltersBtn = document.getElementById('closeFilters');
    const filtersSidebar = document.querySelector('.filters-sidebar');

    if (showFiltersBtn && closeFiltersBtn && filtersSidebar) {
        // Create backdrop element
        const backdrop = document.createElement('div');
        backdrop.className = 'backdrop';
        document.body.appendChild(backdrop);

        showFiltersBtn.addEventListener('click', function () {
            filtersSidebar.classList.add('active');
            backdrop.classList.add('active');
            document.body.classList.add('filters-open');
        });

        closeFiltersBtn.addEventListener('click', function () {
            filtersSidebar.classList.remove('active');
            backdrop.classList.remove('active');
            document.body.classList.remove('filters-open');
        });

        backdrop.addEventListener('click', function () {
            filtersSidebar.classList.remove('active');
            backdrop.classList.remove('active');
            document.body.classList.remove('filters-open');
        });
    }

    // Room filtering
    const applyFiltersBtn = document.getElementById('applyFilters');
    const resetFiltersBtn = document.getElementById('resetFilters');

    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', function () {
            filterRooms();

            // Close mobile filters if open
            if (window.innerWidth < 992) {
                if (filtersSidebar.classList.contains('active')) {
                    filtersSidebar.classList.remove('active');
                    document.querySelector('.backdrop').classList.remove('active');
                    document.body.classList.remove('filters-open');
                }
            }
        });
    }

    if (resetFiltersBtn) {
        resetFiltersBtn.addEventListener('click', function () {
            // Reset price range
            if (priceRangeSlider && priceRangeSlider.noUiSlider) {
                priceRangeSlider.noUiSlider.set([0, 10000000]);
            }

            // Reset room type checkboxes
            document.querySelectorAll('[id^="roomType"]').forEach(checkbox => {
                checkbox.checked = true;
            });

            // Reset amenities
            document.querySelectorAll('[id^="amenity"]').forEach(checkbox => {
                checkbox.checked = false;
            });

            // Reset rating
            document.getElementById('rating0').checked = true;

            // Apply the reset filters
            filterRooms();
        });
    }

    function filterRooms() {
        // Get selected filters
        const minPrice = parseInt(document.getElementById('minPrice').value);
        const maxPrice = parseInt(document.getElementById('maxPrice').value);

        const selectedRoomTypes = [];
        document.querySelectorAll('[id^="roomType"]:checked').forEach(checkbox => {
            selectedRoomTypes.push(checkbox.value);
        });

        const selectedRating = document.querySelector('input[name="ratingFilter"]:checked').value;

        // Filter rooms
        const rooms = document.querySelectorAll('.room-item');

        rooms.forEach(room => {
            const roomPrice = parseInt(room.getAttribute('data-price'));
            const roomRating = parseFloat(room.getAttribute('data-rating'));
            const roomType = room.getAttribute('data-type');

            let isVisible = true;

            // Price filter
            if (roomPrice < minPrice || roomPrice > maxPrice) {
                isVisible = false;
            }

            // Room type filter
            if (!selectedRoomTypes.includes(roomType)) {
                isVisible = false;
            }

            // Rating filter
            if (selectedRating > 0 && roomRating < parseInt(selectedRating)) {
                isVisible = false;
            }

            // Apply visibility
            room.style.display = isVisible ? 'block' : 'none';
        });
    }

    // Room sorting
    const sortOptions = document.querySelectorAll('.sort-option');

    if (sortOptions.length > 0) {
        sortOptions.forEach(option => {
            option.addEventListener('click', function () {
                const sortBy = this.getAttribute('data-sort');
                sortRooms(sortBy);

                // Update dropdown text
                document.getElementById('sortDropdown').textContent = this.textContent;
            });
        });
    }

    function sortRooms(sortBy) {
        const roomsList = document.querySelector('.rooms-list');
        const rooms = Array.from(document.querySelectorAll('.room-item'));

        rooms.sort((a, b) => {
            switch (sortBy) {
                case 'price-asc':
                    return parseInt(a.getAttribute('data-price')) - parseInt(b.getAttribute('data-price'));
                case 'price-desc':
                    return parseInt(b.getAttribute('data-price')) - parseInt(a.getAttribute('data-price'));
                case 'rating-desc':
                    return parseFloat(b.getAttribute('data-rating')) - parseFloat(a.getAttribute('data-rating'));
                case 'popular':
                    // For demo purposes, we'll use rating as a proxy for popularity
                    return parseFloat(b.getAttribute('data-rating')) - parseFloat(a.getAttribute('data-rating'));
                default:
                    return 0;
            }
        });

        // Clear current rooms and append sorted ones
        roomsList.innerHTML = '';
        rooms.forEach(room => roomsList.appendChild(room));
    }

    // Favorite toggle
    const favoriteBtns = document.querySelectorAll('.favorite-btn');

    if (favoriteBtns.length > 0) {
        favoriteBtns.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();

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

                // Here you would send AJAX request to backend to update favorites
                // For now, we'll just toggle the icon
                const roomId = this.getAttribute('data-room-id');
                console.log('Toggle favorite for room:', roomId);
            });
        });
    }

    // Book now button click
    const bookButtons = document.querySelectorAll('.book-now-btn');

    if (bookButtons.length > 0) {
        bookButtons.forEach(btn => {
            btn.addEventListener('click', function () {
                const roomId = this.getAttribute('data-room-id');

                // Get the dates from search form
                const checkin = document.getElementById('checkin').value;
                const checkout = document.getElementById('checkout').value;
                const adults = document.getElementById('adults').value;
                const children = document.getElementById('children').value;

                // Redirect to booking page
                window.location.href = `/Bookings/Create?roomId=${roomId}&checkin=${checkin}&checkout=${checkout}&adults=${adults}&children=${children}`;
            });
        });
    }

    // Toast notification helper
    function showToast(message) {
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
        toast.className = 'toast align-items-center text-white bg-primary border-0';
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