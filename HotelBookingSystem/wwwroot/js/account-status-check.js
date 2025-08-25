// Account status check for AJAX requests
document.addEventListener('DOMContentLoaded', function () {
    // Kiểm tra trạng thái tài khoản ngay khi trang load nếu user đã đăng nhập
    if (window.isUserAuthenticated) {
        checkAccountStatus();
    }

    // Override jQuery AJAX to handle account deactivation
    if (typeof $ !== 'undefined') {
        $(document).ajaxError(function (event, xhr, settings) {
            if (xhr.status === 401 && xhr.responseJSON && xhr.responseJSON.error) {
                showAccountDeactivatedModal(xhr.responseJSON.error, xhr.responseJSON.redirectUrl);
            }
        });
    }

    // Override fetch API to handle account deactivation
    const originalFetch = window.fetch;
    window.fetch = function (...args) {
        return originalFetch.apply(this, args)
            .then(response => {
                if (response.status === 401) {
                    response.clone().json().then(data => {
                        if (data.error && data.redirectUrl) {
                            showAccountDeactivatedModal(data.error, data.redirectUrl);
                        }
                    }).catch(() => {
                        // If not JSON, redirect to login
                        window.location.href = '/Account/Login?message=account_deactivated';
                    });
                }
                return response;
            });
    };
});

// Hàm kiểm tra trạng thái tài khoản
function checkAccountStatus() {
    fetch('/Account/CheckStatus', {
        method: 'GET',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        }
    })
        .then(response => {
            if (response.status === 401) {
                return response.json().then(data => {
                    showAccountDeactivatedModal(data.error, data.redirectUrl);
                    throw new Error('Account deactivated');
                });
            }
            return response.json();
        })
        .then(data => {
            if (!data.isActive) {
                showAccountDeactivatedModal(
                    "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.",
                    "/Account/Login?message=account_deactivated"
                );
            }
        })
        .catch(error => {
            if (error.message !== 'Account deactivated') {
                console.warn('Could not check account status:', error);
            }
        });
}

function showAccountDeactivatedModal(message, redirectUrl) {
    // Create modal if not exists
    let modal = document.getElementById('accountDeactivatedModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'accountDeactivatedModal';
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('aria-labelledby', 'accountDeactivatedModalLabel');
        modal.setAttribute('aria-hidden', 'true');
        modal.setAttribute('data-bs-backdrop', 'static');
        modal.setAttribute('data-bs-keyboard', 'false');

        modal.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-danger text-white">
                        <h5 class="modal-title" id="accountDeactivatedModalLabel">
                            <i class="fas fa-exclamation-triangle me-2"></i>Tài khoản bị khóa
                        </h5>
                    </div>
                    <div class="modal-body">
                        <div class="text-center">
                            <i class="fas fa-user-lock fa-3x text-danger mb-3"></i>
                            <p class="mb-3">${message}</p>
                            <p class="text-muted">Bạn sẽ được chuyển về trang đăng nhập.</p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" onclick="redirectToLogin('${redirectUrl}')">
                            <i class="fas fa-sign-in-alt me-2"></i>Đăng nhập lại
                        </button>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    } else {
        // Update message
        const messageElement = modal.querySelector('.modal-body p');
        if (messageElement) {
            messageElement.textContent = message;
        }

        // Update redirect URL in button
        const button = modal.querySelector('.btn-primary');
        if (button) {
            button.setAttribute('onclick', `redirectToLogin('${redirectUrl}')`);
        }
    }

    // Show modal
    if (typeof bootstrap !== 'undefined') {
        const modalInstance = new bootstrap.Modal(modal);
        modalInstance.show();
    } else {
        // Fallback if Bootstrap is not available
        modal.style.display = 'block';
        modal.classList.add('show');
    }
}

function redirectToLogin(redirectUrl) {
    window.location.href = redirectUrl || '/Account/Login?message=account_deactivated';
}

// For legacy jQuery users
if (typeof $ !== 'undefined') {
    $(document).ready(function () {
        // Handle form submissions that might return 401
        $(document).on('submit', 'form', function (e) {
            const form = this;
            const originalAction = form.action;

            // If it's an AJAX form, let the AJAX error handler deal with it
            if ($(form).hasClass('ajax-form') || $(form).data('ajax')) {
                return;
            }

            // For regular form submissions, we'll rely on the middleware redirect
        });
    });
}
