// Enhanced swipe functionality with better error handling and animations

window.addClass = (selector, className) => {
    try {
        const element = document.querySelector(selector);
        if (element && !element.classList.contains(className)) {
            element.classList.add(className);
            return true;
        }
        return false;
    } catch (error) {
        console.warn(`Failed to add class ${className} to ${selector}:`, error);
        return false;
    }
};

window.removeClass = (selector, className) => {
    try {
        const element = document.querySelector(selector);
        if (element && element.classList.contains(className)) {
            element.classList.remove(className);
            return true;
        }
        return false;
    } catch (error) {
        console.warn(`Failed to remove class ${className} from ${selector}:`, error);
        return false;
    }
};

window.hasClass = (selector, className) => {
    try {
        const element = document.querySelector(selector);
        return element ? element.classList.contains(className) : false;
    } catch (error) {
        console.warn(`Failed to check class ${className} on ${selector}:`, error);
        return false;
    }
};

// Enhanced swipe state management with visual feedback
window.setSwipeState = (selector, state) => {
    try {
        const element = document.querySelector(selector);
        if (!element) return false;

        // Remove all swipe-related classes
        element.classList.remove('swipe-delete', 'swipe-threshold', 'swipe-reset', 'swiping');
        
        // Apply new state with appropriate class
        switch (state) {
            case 'swiping':
                element.classList.add('swiping');
                break;
            case 'delete':
                element.classList.add('swipe-delete');
                // Add pulse animation to delete button
                setTimeout(() => {
                    const deleteAction = element.nextElementSibling;
                    if (deleteAction && deleteAction.classList.contains('delete-action')) {
                        const btn = deleteAction.querySelector('.btn');
                        if (btn) btn.classList.add('pulse-animation');
                    }
                }, 100);
                break;
            case 'threshold':
                element.classList.add('swipe-threshold');
                // Provide haptic feedback if available
                if (navigator.vibrate) {
                    navigator.vibrate(50);
                }
                break;
            case 'reset':
                element.classList.add('swipe-reset');
                // Clean up after animation
                setTimeout(() => element.classList.remove('swipe-reset'), 400);
                break;
            default:
                // Reset state
                break;
        }
        return true;
    } catch (error) {
        console.warn(`Failed to set swipe state ${state} on ${selector}:`, error);
        return false;
    }
};

// スワイプ状態をリセットする関数 - Enhanced with error handling
window.resetAllSwipeStates = () => {
    try {
        const swipedCards = document.querySelectorAll('.memo-card.swipe-delete, .memo-card.swipe-threshold');
        swipedCards.forEach(card => {
            window.setSwipeState(`[data-memo-id="${card.closest('[data-memo-id]')?.dataset.memoId}"] .memo-card`, 'reset');
        });
        return true;
    } catch (error) {
        console.warn('Failed to reset swipe states:', error);
        return false;
    }
};

// Enhanced delete confirmation with better UX
window.confirmDelete = (memoTitle) => {
    try {
        return new Promise((resolve) => {
            // Create custom confirmation dialog with better styling
            const confirmed = confirm(`メモ「${memoTitle}」を削除しますか？\n\nこの操作は取り消せません。`);
            resolve(confirmed);
        });
    } catch (error) {
        console.warn('Failed to show delete confirmation:', error);
        return Promise.resolve(false);
    }
};

// タッチイベントの改善された処理
window.initSwipeHandlers = () => {
    let touchStartX = 0;
    let touchStartY = 0;
    let isSwiping = false;
    
    document.addEventListener('touchstart', (e) => {
        if (e.target.closest('.memo-card')) {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
            isSwiping = false;
        }
    }, { passive: true });
    
    document.addEventListener('touchmove', (e) => {
        if (!e.target.closest('.memo-card')) return;
        
        const touchX = e.touches[0].clientX;
        const touchY = e.touches[0].clientY;
        const deltaX = touchStartX - touchX;
        const deltaY = Math.abs(touchStartY - touchY);
        
        // 水平スワイプの検出
        if (Math.abs(deltaX) > 10 && Math.abs(deltaX) > deltaY * 2) {
            isSwiping = true;
            // 縦スクロールを防ぐ
            e.preventDefault();
        }
    }, { passive: false });
    
    document.addEventListener('touchend', () => {
        isSwiping = false;
    }, { passive: true });
};

// ページロード時に初期化
document.addEventListener('DOMContentLoaded', () => {
    window.initSwipeHandlers();
});