let selectedRating = 0;

function openPopup(bookingId) {
    document.getElementById('BookingId').value = bookingId;
    document.getElementById('reviewPopup').style.display = 'block';

    selectedRating = 0;
    document.getElementById('ratingValue').value = 0;
    document.getElementById('comment').value = '';
    updateStars();
}

function closePopup() {
    document.getElementById('reviewPopup').style.display = 'none';
}

function setRating(rating) {
    selectedRating = rating;
    document.getElementById('ratingValue').value = rating;
    updateStars();
}

function updateStars() {
    const stars = document.querySelectorAll('.star-item');
    stars.forEach((star, index) => {
        if (index < selectedRating) {
            star.classList.add('active');
        } else {
            star.classList.remove('active');
        }
    });
}

function validateReview() {
    console.log(">>> validateReview mới đã chạy <<<");
    const rating = document.getElementById('ratingValue').value;
    const errorEl = document.getElementById('ratingError');

    if (rating == 0) {
        errorEl.style.display = "block";
        return false;
    }

    errorEl.style.display = "none";
    return true;
}

function logSubmitInfo() {
    console.log("booking id: ", document.getElementById('BookingId').value)
    console.log("rating: ", document.getElementById('ratingValue').value)
    console.log("comment: ", document.getElementById('comment').value)
}

// Đóng popup khi click bên ngoài
document.getElementById('reviewPopup').addEventListener('click', function (e) {
    if (e.target === this) {
        closePopup();
    }
});
function showNotification() {
    const notification = document.getElementById('notification');
    if (notification) {
        // Hiển thị notification
        notification.classList.add('show');

        // Tự động ẩn sau 2 giây
        setTimeout(function () {
            notification.classList.remove('show');
            notification.classList.add('hide');

            // Xóa element sau khi animation kết thúc
            setTimeout(function () {
                notification.remove();
            }, 300);
        }, 2000);
    }
}
