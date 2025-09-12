// スワイプ機能用のJavaScriptヘルパー関数

window.addClass = (selector, className) => {
    const element = document.querySelector(selector);
    if (element) {
        element.classList.add(className);
    }
};

window.removeClass = (selector, className) => {
    const element = document.querySelector(selector);
    if (element) {
        element.classList.remove(className);
    }
};

window.hasClass = (selector, className) => {
    const element = document.querySelector(selector);
    return element ? element.classList.contains(className) : false;
};

// スワイプ状態をリセットする関数
window.resetAllSwipeStates = () => {
    const swipedCards = document.querySelectorAll('.memo-card.swipe-delete');
    swipedCards.forEach(card => {
        card.classList.remove('swipe-delete');
    });
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