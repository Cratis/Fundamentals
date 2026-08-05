// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from '../ConceptAs';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Tag extends ConceptAs<string> {}

class Article {
    @field(Tag)
    primaryTag!: Tag;

    @field(Tag, true)
    tags!: Tag[];

    @field(ValueMap, { genericArguments: [String, Tag] })
    tagBySection!: ValueMap<string, Tag>;
}

describe('when round tripping concepts outside a declared field', () => {
    const article = new Article();
    article.primaryTag = new Tag('news');
    article.tags = [new Tag('sport'), new Tag('weather')];
    article.tagBySection = new ValueMap<string, Tag>().set('front', new Tag('lead'));

    const written = JsonSerializer.serialize(article);
    const read = JsonSerializer.deserialize(Article, written);

    it('should write a concept in a collection as its underlying value', () => written.should.contain('"tags":["sport","weather"]'));
    it('should write a concept in a value map as its underlying value', () => written.should.contain('"tagBySection":{"front":"lead"}'));
    it('should write a concept in a declared field as its underlying value', () => written.should.contain('"primaryTag":"news"'));

    it('should read a collection back as concepts', () => read.tags.map(_ => _.value).should.deep.equal(['sport', 'weather']));
    it('should read a collection element back as the concept type', () => read.tags[0].should.be.instanceof(Tag));
    it('should read a value map value back as a concept', () => read.tagBySection.get('front')!.value.should.equal('lead'));
    it('should read a value map value back as the concept type', () => read.tagBySection.get('front')!.should.be.instanceof(Tag));
});
