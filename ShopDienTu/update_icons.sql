UPDATE Categories SET Icon = 'bi-phone' WHERE CategoryName LIKE N'%Điện thoại%' OR CategoryName LIKE N'%Dien thoai%';
UPDATE Categories SET Icon = 'bi-laptop' WHERE CategoryName LIKE N'%Laptop%';
UPDATE Categories SET Icon = 'bi-smartwatch' WHERE CategoryName LIKE N'%Đồng hồ%' OR CategoryName LIKE N'%Dong ho%';
UPDATE Categories SET Icon = 'bi-tablet' WHERE CategoryName LIKE N'%Tablet%' OR CategoryName LIKE N'%Máy tính bảng%';
UPDATE Categories SET Icon = 'bi-headphones' WHERE CategoryName LIKE N'%Tai nghe%';
UPDATE Categories SET Icon = 'bi-camera' WHERE CategoryName LIKE N'%Camera%' OR CategoryName LIKE N'%Máy ảnh%';
UPDATE Categories SET Icon = 'bi-tv' WHERE CategoryName LIKE N'%TV%' OR CategoryName LIKE N'%Tivi%';
UPDATE Categories SET Icon = 'bi-keyboard' WHERE CategoryName LIKE N'%Bàn phím%' OR CategoryName LIKE N'%Phím%';
UPDATE Categories SET Icon = 'bi-mouse' WHERE CategoryName LIKE N'%Chuột%';
-- Default icon fallbacks:
UPDATE Categories SET Icon = 'bi-grid' WHERE Icon IS NULL OR Icon = '';
