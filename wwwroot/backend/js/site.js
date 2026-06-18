$(function () {

    // Xác nhận trước khi xóa
    if ($("a.confirmDeletion").length) {
        $("a.confirmDeletion").click(() => {
            if (!confirm("Bạn có chắc chắn muốn xóa không?")) return false;
        });
    }

    // Tự động ẩn thông báo sau 2 giây
    if ($("div.alert.notification").length) {
        setTimeout(() => {
            $("div.alert.notification").fadeOut();
        }, 2000);
    }

});

// Thấy trước hình ảnh khi bỏ ảnh vô
function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(200).height(200);
        };

        reader.readAsDataURL(input.files[0]);
    }
}
