// set-bg
document.querySelectorAll('.set-bg').forEach(function (el) {
    let bg = el.getAttribute('data-setbg');
    if (bg) el.style.backgroundImage = `url('${bg}')`;
});

// cập nhật UI
function updateCartCount(n) {
    document.getElementById('cartCount').textContent = n;
}

// =========================
//  THÊM GIỎ HÀNG -> reload count từ DB
// =========================
document.body.addEventListener('click', function (e) {

    let a = e.target.closest('a');
    if (!a) return;

    let href = a.getAttribute('href');
    let match = href.match(/\/Cart\/Add\/(\d+)/);

    if (match) {

        // Chờ server xử lý xong -> lấy lại số lượng thật từ DB

        setTimeout(() => {
            fetch('/Cart/GetCount')
                .then(res => res.json())
                .then(count => updateCartCount(count));
        }, 300);
    }
});

// Load số lượng khi mở trang
document.addEventListener("DOMContentLoaded", function () {
    fetch('/Cart/GetCount')
        .then(res => res.json())
        .then(count => {
            document.getElementById('cartCount').textContent = count;
        });
});



// =============================
//  ĐỔI ẢNH CHÍNH KHI CLICK ẢNH NHỎ
// =============================
document.addEventListener("DOMContentLoaded", function () {

    let mainImg = document.querySelector(".product__details__pic__item--large");
    let thumbnails = document.querySelectorAll(".product__details__pic__slider img");

    thumbnails.forEach(img => {
        img.addEventListener("click", function () {
            let bigImgUrl = img.getAttribute("data-imgbigurl");
            if (bigImgUrl) {
                mainImg.src = bigImgUrl;
            }
        });
    });

    // =============================
    //   NÚT TĂNG – GIẢM SỐ LƯỢNG
    // =============================
    document.querySelectorAll('.pro-qty').forEach(qtyBox => {

        let minusBtn = document.createElement("span");
        minusBtn.innerText = "-";

        let plusBtn = document.createElement("span");
        plusBtn.innerText = "+";

        qtyBox.prepend(minusBtn);
        qtyBox.append(plusBtn);

        let input = qtyBox.querySelector("input");

        minusBtn.addEventListener("click", function () {
            let val = parseInt(input.value);
            if (val > 1) input.value = val - 1;
        });

        plusBtn.addEventListener("click", function () {
            let val = parseInt(input.value);
            input.value = val + 1;
        });
    });

});


// =============================
//   TĂNG GIẢM SỐ LƯỢNG TRONG TRANG CHI TIẾT
// =============================
document.addEventListener("DOMContentLoaded", function () {

    let input = document.querySelector(".quantity-input");
    let btnMinus = document.querySelector(".btn-minus");
    let btnPlus = document.querySelector(".btn-plus");

    if (input && btnMinus && btnPlus) {
        btnMinus.addEventListener("click", function () {
            let val = parseInt(input.value);
            if (val > 1) input.value = val - 1;
        });

        btnPlus.addEventListener("click", function () {
            let val = parseInt(input.value);
            input.value = val + 1;
        });
    }

});

document.addEventListener("DOMContentLoaded", function () {

    let qtyInput = document.querySelector(".quantity-input");
    let qtyHidden = document.getElementById("quantityHidden");

    if (qtyInput && qtyHidden) {

        // Khi thay đổi số lượng trong input → cập nhật hidden input
        qtyInput.addEventListener("input", function () {
            let val = parseInt(qtyInput.value);
            qtyHidden.value = (val > 0 ? val : 1);
        });

        // Khi bấm nút thêm giỏ → cập nhật quantity lần cuối
        document.querySelector("form[method='get']").addEventListener("submit", function () {
            let val = parseInt(qtyInput.value);
            qtyHidden.value = (val > 0 ? val : 1);
        });
    }
});
