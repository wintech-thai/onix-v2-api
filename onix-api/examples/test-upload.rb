#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'

$stdout.sync = true

# load_env.rb
File.readlines(".env", chomp: true).each do |line|
  next if line.strip.empty? || line.start_with?('#')

  key, value = line.split('=', 2)
  ENV[key] = value
  
end

def json?(str)
  JSON.parse(str)
  true
rescue JSON::ParserError
  false
end

def make_request(method, apiName, data)
  host = ENV['API_HTTP_ENDPOINT']
  apiKey = ENV['API_KEY']

  uri = URI.parse("#{host}/#{apiName}")  

  # แปลง method เช่น "post" → "Net::HTTP::Post"
  klass_name = "Net::HTTP::#{method.to_s.capitalize}"
  request_class = Object.const_get(klass_name)

  request = request_class.new(uri.request_uri)
  request['Content-Type'] = 'application/json'
  request.basic_auth("api", apiKey)

  if (!data.nil?)
    request.body = data.to_json
  end

  http = Net::HTTP.new(uri.host, uri.port)  
  http.use_ssl = true

  response = http.request(request)

  if (response.code != '200')
    puts("ERROR : Failed to send request with error [#{response}]")
    return
  end

  result = response.body
  if json?(result)
    result = JSON.parse(result)
  end

  return result
end

def upload_file_to_gcs(presigned_url, file_path, content_type)
  uri = URI.parse(presigned_url)

  http = Net::HTTP.new(uri.host, uri.port)
  http.use_ssl = (uri.scheme == "https")

  request = Net::HTTP::Put.new(uri.request_uri)
  request['Content-Type'] = content_type
  request['onix-custom-is-temp'] = 'true'
  request.body = File.read(file_path, mode: "rb")   # อ่านเป็น binary

  response = http.request(request)

  if response.is_a?(Net::HTTPSuccess)
    puts "✅ Upload สำเร็จ: #{file_path}"
  else
    puts "❌ Upload ล้มเหลว: #{response.code} #{response.message}"
    puts response.body
  end
end

################### Main #######################

orgId = "napbiotec"
itemId = "99b5dbd5-7a79-4560-9a89-8c24b0393229"
imageFile = "file_example_PNG_500kB.png"

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

### GetItemImagesByItemId
apiGetItemImagesUrl = "api/Item/org/#{orgId}/action/GetItemImagesByItemId/#{itemId}"
result = make_request(:get, apiGetItemImagesUrl, nil)

image = result[0]
newPreviewUrl = image["imageUrl"]

puts("New preview URL : [#{newPreviewUrl}]")
