document.addEventListener('DOMContentLoaded', () => {
    const desktopProfile = document.querySelector('[data-component="header-profile"]');
    const desktopToggle = desktopProfile?.querySelector('[data-action="toggle-profile-dropdown"]');
    const desktopDropdown = desktopProfile?.querySelector('[data-element="profile-dropdown"]');
    const overlay = document.querySelector('[data-overlay="header-profile"]');
    const overlayCloseButtons = overlay ? overlay.querySelectorAll('[data-action="header-close-overlay"]') : [];
    const overlayList = overlay?.querySelector('[data-element="header-profile-list"]');
    const createForm = document.getElementById('headerProfileCreateForm');
    const createInput = document.getElementById('headerProfileCreateName');
    const setForm = document.getElementById('headerProfileSetForm');
    const openCreateButtons = document.querySelectorAll('[data-action="header-open-create"]');
    const openMenuButtons = document.querySelectorAll('[data-action="header-open-menu"]');

    const getAntiForgeryToken = () => {
        const fromCreate = createForm?.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (fromCreate) {
            return fromCreate;
        }
        return setForm?.querySelector('input[name="__RequestVerificationToken"]')?.value ?? null;
    };

    const closeDesktopDropdown = () => {
        if (!desktopDropdown || !desktopToggle) {
            return;
        }
        desktopDropdown.hidden = true;
        desktopToggle.setAttribute('aria-expanded', 'false');
        desktopToggle.classList.remove('is-open');
        document.removeEventListener('click', handleOutsideClick);
    };

    const handleOutsideClick = (event) => {
        if (!desktopProfile || desktopProfile.contains(event.target)) {
            return;
        }
        closeDesktopDropdown();
    };

    const toggleDesktopDropdown = () => {
        if (!desktopDropdown || !desktopToggle) {
            return;
        }
        const isHidden = desktopDropdown.hasAttribute('hidden');
        if (isHidden) {
            desktopDropdown.hidden = false;
            desktopToggle.setAttribute('aria-expanded', 'true');
            desktopToggle.classList.add('is-open');
            window.setTimeout(() => {
                document.addEventListener('click', handleOutsideClick);
            }, 0);
        } else {
            closeDesktopDropdown();
        }
    };

    desktopToggle?.addEventListener('click', (event) => {
        event.preventDefault();
        toggleDesktopDropdown();
    });

    const toggleBodyScroll = (disable) => {
        if (disable) {
            document.body.dataset.headerProfileOverlay = 'open';
            document.body.style.overflow = 'hidden';
        } else {
            delete document.body.dataset.headerProfileOverlay;
            document.body.style.overflow = '';
        }
    };

    const openOverlay = () => {
        if (!overlay) {
            return;
        }
        closeDesktopDropdown();
        overlay.hidden = false;
        overlay.setAttribute('aria-hidden', 'false');
        toggleBodyScroll(true);
        window.setTimeout(() => createInput?.focus(), 80);
    };

    const closeOverlay = () => {
        if (!overlay) {
            return;
        }
        overlay.hidden = true;
        overlay.setAttribute('aria-hidden', 'true');
        toggleBodyScroll(false);
    };

    openCreateButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            openOverlay();
        });
    });

    openMenuButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            openOverlay();
        });
    });

    overlayCloseButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            closeOverlay();
        });
    });

    overlay?.addEventListener('click', (event) => {
        if (event.target === overlay) {
            closeOverlay();
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            closeOverlay();
            closeDesktopDropdown();
        }
    });

    const handleSuccess = (message) => {
        if (window.toastr) {
            window.toastr.success(message || 'عملیات با موفقیت انجام شد');
        }
        window.setTimeout(() => window.location.reload(), 250);
    };

    const handleFailure = (error) => {
        if (window.toastr) {
            window.toastr.error(error || 'خطا در انجام عملیات');
        }
    };

    const submitProfileChange = async (profileIdValue) => {
        if (!setForm) {
            return;
        }

        const token = getAntiForgeryToken();
        if (!token) {
            handleFailure('توکن امنیتی یافت نشد');
            return;
        }

        const formData = new FormData();
        formData.append('profileId', profileIdValue ?? '');
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(setForm.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`Request failed with status ${response.status}`);
            }

            const result = await response.json();
            if (result?.success) {
                handleSuccess(result.message);
            } else {
                handleFailure(result?.error);
            }
        } catch (error) {
            console.error('Failed to set active profile', error);
            handleFailure('خطا در برقراری ارتباط با سرور');
        }
    };

    const attachProfileSetListeners = (root) => {
        root.querySelectorAll('[data-action="header-profile-set"]').forEach((button) => {
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                const profileId = button.getAttribute('data-profile-id') ?? '';
                await submitProfileChange(profileId);
            });
        });
    };

    if (desktopDropdown) {
        attachProfileSetListeners(desktopDropdown);
    }

    if (overlayList) {
        attachProfileSetListeners(overlayList);
    }

    createForm?.addEventListener('submit', async (event) => {
        event.preventDefault();

        const nameValue = createInput?.value?.trim();
        if (!nameValue) {
            handleFailure('لطفاً یک نام برای پروفایل وارد کنید');
            createInput?.focus();
            return;
        }

        const token = getAntiForgeryToken();
        if (!token) {
            handleFailure('توکن امنیتی یافت نشد');
            return;
        }

        const formData = new FormData();
        formData.append('displayName', nameValue);
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(createForm.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`Request failed with status ${response.status}`);
            }

            const result = await response.json();
            if (result?.success) {
                handleSuccess(result.message);
            } else {
                handleFailure(result?.error);
            }
        } catch (error) {
            console.error('Failed to create profile', error);
            handleFailure('خطا در برقراری ارتباط با سرور');
        }
    });
});

