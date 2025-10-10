#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = "napbiotec"
itemId = "99b5dbd5-7a79-4560-9a89-8c24b0393229"
imageFile = "file_example_PNG_500kB.png" #"1729533908_14096.jpg" 

### DeleteItemImagesByItemId
#apiDeleteItemImagesUrl = "api/Item/org/#{orgId}/action/DeleteItemImagesByItemId/#{itemId}"
#result = make_request(:delete, apiDeleteItemImagesUrl, nil)
#puts(result)

### GetItemImageUploadPresignedUrl
apiGetPresignUrl = "api/Item/org/#{orgId}/action/GetItemImageUploadPresignedUrl/#{itemId}"
result = make_request(:get, apiGetPresignUrl, nil)

imagePath = result["imagePath"]
presignedUrl = result["presignedUrl"]
previewUrl = result["previewUrl"]

### Upload file to GCS
puts("Image path  : [#{imagePath}]")
upload_file_to_gcs(presignedUrl, imageFile, 'image/png')

#puts("Preview URL : [#{previewUrl}]") # อันนี้เอาไว้ใช้สำหรับ preview ในหน้าเว็บหลังจากที่ upload file เสร็จแล้วแต่ยังไม่ได้ AddItemImage() จริง ๆ

### AddItemImage
apiAddItemImageUrl = "api/Item/org/#{orgId}/action/AddItemImage/#{itemId}"
itemImage =  {
  imagePath: imagePath,
  category: 1,
  narative: "ทดสอบอัพโหลดรูป",
  tags: "ทดสอบ",
  sortingOrder: 1,
}
result = make_request(:post, apiAddItemImageUrl, itemImage)
puts("Added image status : [#{result['status']}] [#{result['description']}]")

### GetItemImagesByItemId
#apiGetItemImagesUrl = "api/Item/org/#{orgId}/action/GetItemImagesByItemId/#{itemId}"
#result = make_request(:get, apiGetItemImagesUrl, nil)

#image = result[0]
#newPreviewUrl = image["imageUrl"]

#puts("New preview URL : [#{newPreviewUrl}]")
